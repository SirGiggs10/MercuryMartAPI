using AutoMapper;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.Administrator;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Dtos.RoleFunctionality;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class AdministratorRepository : IAdministratorRepository
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IGlobalRepository _globalRepository;
        private readonly IAuthRepository _authRepository;
        private readonly IMailRepository _mailRepository;
        private readonly IRoleManagementRepository _roleManagementRepository;
        private readonly IConfiguration _configuration;
        private readonly Helper _helper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public AdministratorRepository(DataContext dataContext, UserManager<User> userManager, RoleManager<Role> roleManager, IGlobalRepository globalRepository, IAuthRepository authRepository, IMailRepository mailRepository, IRoleManagementRepository roleManagementRepository, IConfiguration configuration, Helper helper, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _roleManager = roleManager;
            _globalRepository = globalRepository;
            _authRepository = authRepository;
            _mailRepository = mailRepository;
            _roleManagementRepository = roleManagementRepository;
            _configuration = configuration;
            _helper = helper;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> CreateAdministrator(AdministratorRequest administratorRequest)
        {
            //REGISTER Administrator
            if (administratorRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if(string.IsNullOrWhiteSpace(administratorRequest.EmailAddress) || string.IsNullOrWhiteSpace(administratorRequest.FullName))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var roleToFind = await _roleManager.FindByIdAsync(Convert.ToString(administratorRequest.RoleId));
            if (roleToFind == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = "Role Not Found"
                };
            }

            if (await _authRepository.UserEmailExists(administratorRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectExists,
                    StatusMessage = "Email already exist(s)!"
                };
            }

            var Administrator = new Administrator
            {
                EmailAddress = administratorRequest.EmailAddress,
                FullName = administratorRequest.FullName,
            };

            var additionResult = _globalRepository.Add(Administrator);
            if(!additionResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = "Error Adding Administrator"
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if (saveResult.HasValue)
            {
                if (!saveResult.Value)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.SaveNoRowAffected,
                        StatusMessage = "Administrator Information Could Not Save"
                    };
                }

                var user = new User
                {
                    UserName = administratorRequest.EmailAddress,
                    Email = administratorRequest.EmailAddress,
                    UserTypeId = Administrator.AdministratorId,
                    UserType = Utils.Administrator
                };

                var password = _helper.RandomPassword();
                var result = await _userManager.CreateAsync(user, password);
                if (result.Succeeded)
                {
                    //THEN UPDATE Administrator TABLE USERID COLUMN WITH NEWLY CREATED USER ID
                    Administrator.UserId = user.Id;
                    var administratorUpdateResult = _globalRepository.Update(Administrator);
                    if (!administratorUpdateResult)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = "Error Occured while saving Administrator Information"
                        };
                    }

                    var administratorSaveResult = await _globalRepository.SaveAll();
                    if (!administratorSaveResult.HasValue)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveError,
                            StatusMessage = "Error Occured while saving Administrator Information"
                        };
                    }

                    if (!administratorSaveResult.Value)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.SaveNoRowAffected,
                            StatusMessage = "Error Occured while saving Administrator Information"
                        };
                    }

                    //ASSIGN ROLES FROM THE REQUEST DTO TO USER
                    var assignmentResult = await _roleManagementRepository.AssignRolesToUser(new RoleUserAssignmentRequest()
                    {
                        Users = new List<int>()
                        {
                            user.Id
                        },
                        Roles = new List<int>()
                        {
                            administratorRequest.RoleId
                        }
                    });

                    if (assignmentResult.StatusCode == Utils.Success)
                    {
                        var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                        string hashedEmail = _authRepository.GetHashedEmail(user.Email);
                        string fullToken = userTokenVal + "#" + hashedEmail;
                        var emailVerificationLink = _authRepository.GetUserEmailVerificationLink(fullToken);
                        if (emailVerificationLink == null)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.ObjectNull,
                                StatusMessage = "Could not generate Email Verification Link"
                            };
                        }

                        var emailMessage1 = $"Your default password is {password}. It is recommended that you change this password after Confirming your Account and Logging In";
                        var emailMessage2 = "Please click the button below to complete your registration and activate your account.";
                        var emailBody = _globalRepository.GetMailBodyTemplate(Administrator.FullName, "", emailVerificationLink, emailMessage1, emailMessage2, "activation.html");
                        var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";
                        //SEND MAIL TO ADMINISTRATOR TO VERIFY EMAIL
                        MailModel mailObj = new MailModel(_configuration.GetValue<string>("MercuryMartEmailAddress"), _configuration.GetValue<string>("MercuryMartEmailName"), Administrator.EmailAddress, emailSubject, emailBody);
                        var response = await _mailRepository.SendMail(mailObj);
                        if (!response.StatusCode.Equals(HttpStatusCode.Accepted))
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.MailFailure,
                                StatusMessage = "Error Occured while sending Mail to Administrator"
                            };
                        }

                        var administratorToReturn = await GetAdministrators(Administrator.AdministratorId);
                        if (administratorToReturn.StatusCode != Utils.Success)
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = "Error Occured while Fetching Administrator Information"
                            };
                        }

                        return new ReturnResponse()
                        {
                            StatusCode = Utils.Success,
                            ObjectValue = administratorToReturn,
                            StatusMessage = "Administrator Created Successfully!!!"
                        };
                    }

                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = "Error Occured while saving Administrator Information"
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = "Error Occured while saving Administrator Information"
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SaveError,
                StatusMessage = "Error Occured while saving Administrator Information"
            };
        }

        public async Task<ReturnResponse> DeleteAdministrator(List<int> administratorIds)
        {
            if(administratorIds == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var administratorsToDelete = new List<Administrator>();
            foreach(var t in administratorIds)
            {
                var administrator = await _dataContext.Administrator.Where(a => a.AdministratorId == t).FirstOrDefaultAsync();
                if(administrator == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var user = await _userManager.FindByIdAsync(Convert.ToString(administrator.UserId));
                if (user == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                var userDeletionResult = await _userManager.DeleteAsync(user);
                if(!userDeletionResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                administratorsToDelete.Add(administrator);
            }

            /*var deletionResult = _globalRepository.Delete(administratorsToDelete);
            if(!deletionResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if(!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if(!saveResult.Value)
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
                ObjectValue = administratorsToDelete
            };
        }

        public async Task<ReturnResponse> GetAdministrators(UserParams userParams)
        {
            var administrators = _dataContext.Administrator;

            var pagedListOfAdministrators = await PagedList<Administrator>.CreateAsync(administrators, userParams.PageNumber, userParams.PageSize);
            var listOfAdministrators = pagedListOfAdministrators.ToList();

            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfAdministrators.CurrentPage, pagedListOfAdministrators.PageSize, pagedListOfAdministrators.TotalCount, pagedListOfAdministrators.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = listOfAdministrators
            };
        }

        public async Task<ReturnResponse> GetAdministrators(int administratorId)
        {
            var administrator = await _dataContext.Administrator.Where(a => a.AdministratorId == administratorId).FirstOrDefaultAsync();

            if(administrator == null)
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
                ObjectValue = administrator
            };
        }

        public async Task<ReturnResponse> UpdateAdministrator(int administratorId, AdministratorToUpdate administrator)
        {
            if(administrator == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            //GET USER THAT MADE THIS REQUEST
            var userTypeIdClaim = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.NameIdentifier);
            if(userTypeIdClaim == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            var userTypeIdVal = Convert.ToInt32(userTypeIdClaim.Value);

            if((administratorId != administrator.AdministratorId) || (administratorId != userTypeIdVal))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var administratorDetails = await _dataContext.Administrator.Where(a => a.AdministratorId == administratorId).FirstOrDefaultAsync();
            if(administratorDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var administratorToUpdate = _mapper.Map(administrator, administratorDetails);
            var updateResult = _globalRepository.Update(administratorToUpdate);
            if(!updateResult)
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
                ObjectValue = administratorToUpdate
            };
        }
    }
}
