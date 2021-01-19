using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Customer
{
    public class CustomerResponse
    {
        public int CustomerId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }

        public List<CustomerCartItemResponse> CustomerCartItems { get; set; }
    }
}
