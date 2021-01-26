using AutoMapper;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using CloudinaryDotNet;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Globalization;
using System.Security.Claims;
using MercuryMartAPI.Dtos.RoleFunctionality;

namespace MercuryMartAPI.Repositories
{
    public class CustomerManagementRepository : ICustomerManagementRepository
    {
        private readonly DataContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMailRepository _mailRepository;
        private readonly IConfiguration _configuration;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly UserManager<User> _userManager;
        private readonly Helper _helper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleManagementRepository _roleManagementRepository;
        private readonly RoleManager<Role> _roleManager;
        
        public CustomerManagementRepository(DataContext dataContext, IMapper mapper, UserManager<User> userManager, IGlobalRepository globalRepository, IMailRepository mailRepository, IConfiguration configuration, IAuthRepository authRepository, Helper helper, IHttpContextAccessor httpContextAccessor, IRoleManagementRepository roleManagementRepository, RoleManager<Role> roleManager)
        {
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _mailRepository = mailRepository;
            _userManager = userManager;
            _configuration = configuration;
            _authRepository = authRepository;
            _helper = helper;
            _httpContextAccessor = httpContextAccessor;
            _roleManagementRepository = roleManagementRepository;
            _roleManager = roleManager;
        }

        public async Task<ReturnResponse> GetCustomers(UserParams userParams)
        {
            var customers = _dataContext.Customer;

            var pagedListOfCustomers = await PagedList<Customer>.CreateAsync(customers, userParams.PageNumber, userParams.PageSize);
            var listOfCustomers = pagedListOfCustomers.ToList();
            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfCustomers.CurrentPage, pagedListOfCustomers.PageSize, pagedListOfCustomers.TotalCount, pagedListOfCustomers.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = listOfCustomers,
                StatusMessage = Utils.StatusMessageSuccess
            };
        }

        public async Task<ReturnResponse> CreateCustomer(CustomerRequest customerRequest)
        {
            if (customerRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if(string.IsNullOrWhiteSpace(customerRequest.EmailAddress) || string.IsNullOrWhiteSpace(customerRequest.FullName) || string.IsNullOrWhiteSpace(customerRequest.PhoneNumber) || string.IsNullOrWhiteSpace(customerRequest.Address) || string.IsNullOrWhiteSpace(customerRequest.Password))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var roleToFind = await _roleManager.FindByNameAsync(Utils.CustomerRole);
            if (roleToFind == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = "Role Not Found"
                };
            }

            if (await _authRepository.UserEmailExists(customerRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectExists,
                    StatusMessage = Utils.StatusMessageObjectExists
                };
            }

            var customer = new Customer()
            {
                EmailAddress = customerRequest.EmailAddress,
                FullName = customerRequest.FullName,
                PhoneNumber = customerRequest.PhoneNumber,
                Address = customerRequest.Address
            };

            var user = new User()
            {
                UserName = customerRequest.EmailAddress,
                Email = customerRequest.EmailAddress,
                //UserTypeId = customer.CustomerId,
                UserType = Utils.Customer,
                Customer = customer
            };

            var result = await _userManager.CreateAsync(user, customerRequest.Password);
            if (result.Succeeded)
            {
                //UPDATE USERTYPEID IN USER TABLE
                user.UserTypeId = customer.CustomerId;
                var updateResult = await _userManager.UpdateAsync(user);
                if (!updateResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                //ASSIGN CUSTOMER ROLE TO USER (CUSTOMER)
                var assignmentResult = await _roleManagementRepository.AssignRolesToUser(new RoleUserAssignmentRequest()
                {
                    Users = new List<int>()
                    {
                        user.Id
                    },
                    Roles = new List<int>()
                    {
                        roleToFind.Id
                    }
                });

                if (assignmentResult.StatusCode == Utils.Success)
                {
                    //SEND MAIL TO CUSTOMER TO CONFIRM EMAIL
                    var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                    string hashedEmail = _authRepository.GetHashedEmail(user.Email);
                    var fullToken = userTokenVal + "#" + hashedEmail;
                    var emailVerificationLink = _authRepository.GetUserEmailVerificationLink(fullToken);
                    if (emailVerificationLink == null)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.ObjectNull,
                            StatusMessage = Utils.StatusMessageObjectNull
                        };
                    }

                    var emailMessage1 = "Please click the button below to complete your registration and activate you account.";
                    var emailMessage2 = "";
                    string emailBody = _globalRepository.GetMailBodyTemplate(customer.FullName, "", emailVerificationLink, emailMessage1, emailMessage2, "activation.html");
                    var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";
                    //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                    MailModel mailObj = new MailModel(_configuration.GetValue<string>("MercuryMartEmailAddress"), _configuration.GetValue<string>("MercuryMartEmailName"), customer.EmailAddress, emailSubject, emailBody);
                    var response = await _mailRepository.SendMail(mailObj);
                    if (!response.StatusCode.Equals(HttpStatusCode.Accepted))
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.MailFailure,
                            StatusMessage = Utils.StatusMessageMailFailure
                        };
                    }

                    var customerToReturn = await GetCustomers(customer.CustomerId);
                    if (customerToReturn.StatusCode != Utils.Success)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = "Error Occured while Fetching Customer Information"
                        };
                    }

                    return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Registration Successful!!!",
                        ObjectValue = (Customer)customerToReturn.ObjectValue
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.NotSucceeded,
                StatusMessage = Utils.StatusMessageNotSucceeded
            };
        }
       
        public async Task<ReturnResponse> SearchCustomer(string searchParams, UserParams userParams)
        {
            if (string.IsNullOrEmpty(searchParams))
            {

                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var customers = _dataContext.Customer.Where(s => (CultureInfo.InvariantCulture.CompareInfo.IndexOf(s.EmailAddress, searchParams, CompareOptions.IgnoreCase) >= 0) || s.FullName.Contains(searchParams) || s.PhoneNumber.Contains(searchParams) || s.Address.Contains(searchParams));
            
            var pagedListOfCustomers = await PagedList<Customer>.CreateAsync(customers, userParams.PageNumber, userParams.PageSize);
            var listOfCustomers = pagedListOfCustomers.ToList();
            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfCustomers.CurrentPage, pagedListOfCustomers.PageSize, pagedListOfCustomers.TotalCount, pagedListOfCustomers.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = "Search Successful!!!",
                ObjectValue = listOfCustomers,
            };
        }

        public async Task<ReturnResponse> GetCustomers(int customerId)
        {
            var customer = await _dataContext.Customer.Where(a => a.CustomerId == customerId).FirstOrDefaultAsync();
            
            if(customer == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customer
            };
        }

        public async Task<ReturnResponse> UpdateCustomer(int customerId, CustomerToUpdate customer)
        {
            if (customer == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            //GET USER THAT MADE THIS REQUEST
            var userTypeIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier);
            if (userTypeIdClaim == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            var userTypeIdVal = Convert.ToInt32(userTypeIdClaim.Value);

            if ((customerId != customer.CustomerId) || (customerId != userTypeIdVal))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var customerDetails = await _dataContext.Customer.Where(a => a.CustomerId == customerId).FirstOrDefaultAsync();
            if (customerDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var customerToUpdate = _mapper.Map(customer, customerDetails);
            var updateResult = _globalRepository.Update(customerToUpdate);
            if (!updateResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if (!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerToUpdate
            };
        }

        public async Task<ReturnResponse> DeleteCustomer(List<int> customersIds)
        {
            if (customersIds == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var customersToDelete = new List<Customer>();
            foreach (var t in customersIds)
            {
                var customer = await _dataContext.Customer.Where(a => a.CustomerId == t).FirstOrDefaultAsync();
                if (customer == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var user = await _userManager.FindByIdAsync(Convert.ToString(customer.UserId));
                if (user == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var userDeletionResult = await _userManager.DeleteAsync(user);
                if (!userDeletionResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                customersToDelete.Add(customer);
            }
            /*
            var deletionResult = _globalRepository.Delete(customersToDelete);
            if (!deletionResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if (!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }
            */
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customersToDelete
            };
        }
    }
}
