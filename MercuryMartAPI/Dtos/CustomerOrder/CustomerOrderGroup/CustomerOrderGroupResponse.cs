using MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup.CustomerOrderGroupItem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup
{
    public class CustomerOrderGroupResponse
    {
        public int CustomerOrderGroupId { get; set; }
        public int CustomerOrderId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public int QuantityOrdered { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<CustomerOrderGroupItemResponse> CustomerOrderGroupItems { get; set; }
    }
}
