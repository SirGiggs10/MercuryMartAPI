using MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.CustomerOrder
{
    public class CustomerOrderResponse
    {
        public int CustomerOrderId { get; set; }
        public int CustomerId { get; set; }
        public int OrderStatus { get; set; }
        public int DeliveryStatus { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public List<CustomerOrderGroupResponse> CustomerOrderGroups { get; set; }
    }
}
