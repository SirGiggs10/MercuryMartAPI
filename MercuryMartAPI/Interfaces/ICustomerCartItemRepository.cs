using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface ICustomerCartItemRepository
    {
        public Task<ReturnResponse> CreateCustomerCartItem(CustomerCartItemRequest customerCartItemRequest);
        public Task<ReturnResponse> GetCustomerCartItem();
        public Task<ReturnResponse> DeleteCustomerCartItem(int customerCartItemId);
        public Task<ReturnResponse> DeleteCustomerCartItem();
    }
}
