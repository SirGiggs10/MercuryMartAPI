using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup
{
    public class CustomerOrderGroupRequest
    {
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public int QuantityOrdered { get; set; }
    }
}
