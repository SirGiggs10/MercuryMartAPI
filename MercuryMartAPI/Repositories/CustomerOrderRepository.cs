using AutoMapper;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.CustomerOrder;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class CustomerOrderRepository : ICustomerOrderRepository
    {
        private readonly DataContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;
        private readonly IMailRepository _mailRepository;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CustomerOrderRepository(DataContext dataContext, IGlobalRepository globalRepository, IMapper mapper, IMailRepository mailRepository, IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _mailRepository = mailRepository;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReturnResponse> CreateCustomerOrder(CustomerOrderRequest customerOrderRequest)
        {
            if(customerOrderRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if(customerOrderRequest.CustomerOrderGroupRequests == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var loggedInUser = _globalRepository.GetUserInformation();
            if(loggedInUser.UserTypeId == Utils.UserClaim_Null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            var customer = await _globalRepository.Get<Customer>(loggedInUser.UserTypeId);
            if(customer == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var customerOrder = _mapper.Map<CustomerOrder>(customerOrderRequest);
            customerOrder.CustomerId = customer.CustomerId;

            var assignmentResult = await AssignProductsToCustomerOrderGroupItems(customerOrder.CustomerOrderGroups, customer.CustomerId);
            if(assignmentResult.StatusCode != Utils.Success)
            {
                return assignmentResult;
            }

            customerOrder.CustomerOrderGroups = (List<CustomerOrderGroup>)assignmentResult.ObjectValue;
            customerOrder.OrderStatus = Utils.CurrentOrderNumberStatus_Pending;
            customerOrder.DeliveryStatus = Utils.CurrentOrderNumberStatus_Pending;

            var creationResult = _globalRepository.Add(customerOrder);
            if(!creationResult)
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

            //FINALLY CREATE ORDER AND UPDATE ORDER STATUS
            //UPDATE DELIVERY STATUS
            var updateStatus = await UpdateCustomerOrderStatusAndDeliveryStatus(customerOrder.CustomerOrderId, true, true);
            if(updateStatus.StatusCode != Utils.Success)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            //SEND MAIL TO CUSTOMER WITH ORDER DETAILS
            var emailMessage1 = "Your Order was placed successfully. Thank you for shopping with us.";
            var emailMessage2 = "";
            string emailBody = _globalRepository.GetMailBodyTemplate(customer.FullName, "", "", emailMessage1, emailMessage2, "activation.html");
            var emailSubject = "ORDER PLACED SUCCESSFULLY";
            //SEND MAIL TO CUSTOMER
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

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerOrder
            };
        }

        public async Task<ReturnResponse> DeleteCustomerOrder(List<int> customerOrdersIds)
        {
            throw new NotImplementedException();
        }

        public async Task<ReturnResponse> GetCustomerOrder(UserParams userParams)
        {
            var loggedInUser = _globalRepository.GetUserInformation();
            if (loggedInUser.UserTypeId == Utils.UserClaim_Null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            var customerOrders = _dataContext.CustomerOrder.Where(a => a.CustomerId == loggedInUser.UserTypeId);
            var customerOrdersPagedList = await PagedList<CustomerOrder>.CreateAsync(customerOrders, userParams.PageNumber, userParams.PageSize);
            var customerOrdersList = customerOrdersPagedList.ToList();

            _httpContextAccessor.HttpContext.Response.AddPagination(customerOrdersPagedList.CurrentPage, customerOrdersPagedList.PageSize, customerOrdersPagedList.TotalCount, customerOrdersPagedList.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerOrdersList
            };
        }

        public async Task<ReturnResponse> GetCustomerOrder(int customerOrderId)
        {
            var customerOrder = await _dataContext.CustomerOrder.Where(a => a.CustomerOrderId == customerOrderId).Include(b => b.CustomerOrderGroups).ThenInclude(c => c.CustomerOrderGroupItems).FirstOrDefaultAsync();
            if(customerOrder == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var loggedInUser = _globalRepository.GetUserInformation();
            if(loggedInUser.UserTypeId == Utils.UserClaim_Null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            if(loggedInUser.UserTypeId != customerOrder.CustomerId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerOrder
            };
        }

        public async Task<ReturnResponse> UpdateCustomerOrder(CustomerOrderToUpdate customerOrderToUpdate)
        {
            throw new NotImplementedException();
        }

        private async Task<ReturnResponse> AssignProductsToCustomerOrderGroupItems(List<CustomerOrderGroup> customerOrderGroups, int customerId)
        {
            foreach (var t in customerOrderGroups)
            {
                var customerOrderGroupItems = new List<CustomerOrderGroupItem>();
                //CROSS CHECK TO MAKE SURE THAT AVAILBALE PRODUCT QUANTITY IS ENOUGH FOR THE ORDER..ELSE THROW ERROR
                var products = _dataContext.Product.Where(a => string.Equals(a.ProductName, t.ProductName) && (a.CategoryId == t.CategoryId) && (!a.AssignedTo.HasValue)).OrderBy(a => a.ProductId);
                if((await products.CountAsync()) < t.QuantityOrdered)
                {
                    //INVALID ORDER
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.QuantityNotInStock,
                        StatusMessage = Utils.StatusMessageQuantityNotInStock
                    };
                }

                //ASSIGN THE PRODUCTS
                var productsChosen = await products.Take(t.QuantityOrdered).ToListAsync();
                productsChosen.ForEach(a =>
                {
                    customerOrderGroupItems.Add(new CustomerOrderGroupItem()
                    {
                        ProductId = a.ProductId
                    });

                    a.AssignedTo = customerId;
                });

                t.CustomerOrderGroupItems = customerOrderGroupItems;

                //UPDATE ASSIGNED STATUS OF ASSIGNED PRODUCTS
                var updateStatus = _globalRepository.Update(productsChosen);    //TEST THIS FUNCTION WITH SAME ITEM ADDED 2 TIMES IN THE CART
                if(!updateStatus)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerOrderGroups
            };
        }

        public async Task<ReturnResponse> UpdateCustomerOrderStatusAndDeliveryStatus(int customerOrderId, bool orderStatus, bool deliveryStatus)
        {
            var customerOrder = await _globalRepository.Get<CustomerOrder>(customerOrderId);
            if(customerOrder == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            customerOrder.OrderStatus = Utils.CurrentOrderNumberStatus_Completed;
            customerOrder.DeliveryStatus = Utils.CurrentOrderNumberStatus_Completed;

            //THEN UPDATE THE INFO TO THE DB
            var updateStatus = _globalRepository.Update(customerOrder);
            if (!updateStatus)
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
                ObjectValue = customerOrder
            };
        }
    }
}
