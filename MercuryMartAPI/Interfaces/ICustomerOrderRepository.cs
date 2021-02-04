using MercuryMartAPI.Dtos.CustomerOrder;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface ICustomerOrderRepository
    {
        public Task<ReturnResponse> CreateCustomerOrder(CustomerOrderRequest customerOrderRequest);
        public Task<ReturnResponse> GetCustomerOrder(UserParams userParams);
        public Task<ReturnResponse> GetCustomerOrder(int customerOrderId);
        public Task<ReturnResponse> UpdateCustomerOrder(CustomerOrderToUpdate customerOrderToUpdate);
        public Task<ReturnResponse> DeleteCustomerOrder(List<int> customerOrdersIds);
        public Task<ReturnResponse> UpdateCustomerOrderStatusAndDeliveryStatus(int customerOrderId, bool orderStatus, bool deliveryStatus);
    }
}
