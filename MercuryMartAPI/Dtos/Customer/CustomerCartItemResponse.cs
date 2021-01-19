using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Customer
{
    public class CustomerCartItemResponse
    {
        public int CustomerCartItemId { get; set; }
        public int CustomerId { get; set; }
        public string CategoryName { get; set; }
        public string ProductName { get; set; }
        public int QuantityToOrder { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
