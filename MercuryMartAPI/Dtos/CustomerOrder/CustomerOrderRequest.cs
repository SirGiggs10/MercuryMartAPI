using MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.CustomerOrder
{
    public class CustomerOrderRequest
    {
        public string OrderName { get; set; }
        public List<CustomerOrderGroupRequest> CustomerOrderGroupRequests { get; set; }
    }
}
