using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface ICustomerManagementRepository
    {
        public Task<ReturnResponse> GetCustomers(UserParams userParams);
        public Task<ReturnResponse> CreateCustomer(CustomerRequest customerRequest);
        public Task<ReturnResponse> SearchCustomer(string searchParams, UserParams userParams);
    }
}
