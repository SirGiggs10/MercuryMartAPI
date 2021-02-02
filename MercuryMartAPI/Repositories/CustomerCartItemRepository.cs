using AutoMapper;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class CustomerCartItemRepository : ICustomerCartItemRepository
    {
        private readonly DataContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;

        public CustomerCartItemRepository(DataContext dataContext, IGlobalRepository globalRepository, IMapper mapper)
        {
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> CreateCustomerCartItem(CustomerCartItemRequest customerCartItemRequest)
        {
            if(customerCartItemRequest == null)
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

            var customerCartItem = _mapper.Map<CustomerCartItem>(customerCartItemRequest);
            customerCartItem.CustomerId = loggedInUser.UserTypeId;

            var creationResult = _globalRepository.Add(customerCartItem);
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

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerCartItem
            };
        }

        public async Task<ReturnResponse> DeleteCustomerCartItem(int customerCartItemId)
        {
            var customerCartItem = await _dataContext.CustomerCartItem.Where(a => a.CustomerCartItemId == customerCartItemId).FirstOrDefaultAsync();
            if(customerCartItem == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var loggedInUser = _globalRepository.GetUserInformation();
            if (loggedInUser.UserTypeId == Utils.UserClaim_Null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            if(customerCartItem.CustomerId != loggedInUser.UserTypeId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var deletionResult = _globalRepository.Delete(customerCartItem);
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

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerCartItem
            };
        }

        public async Task<ReturnResponse> DeleteCustomerCartItem()
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

            var customerCartItems = await _dataContext.CustomerCartItem.Where(a => a.CustomerId == loggedInUser.UserTypeId).ToListAsync();

            var deletionResult = _globalRepository.Delete(customerCartItems);
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

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerCartItems
            };
        }

        public async Task<ReturnResponse> GetCustomerCartItem()
        {
            var loggedInUser = _globalRepository.GetUserInformation();
            if(loggedInUser.UserTypeId == Utils.UserClaim_Null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.UserClaimNotFound,
                    StatusMessage = Utils.StatusMessageUserClaimNotFound
                };
            }

            var customerCartItems = await _dataContext.CustomerCartItem.Where(a => a.CustomerId == loggedInUser.UserTypeId).ToListAsync();

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = customerCartItems
            };
        }
    }
}
