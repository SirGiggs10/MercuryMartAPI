using MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.CustomerOrder
{
    public class CustomerOrderToUpdate
    {
        public int CustomerOrderId { get; set; }
        public List<CustomerOrderGroupToUpdate> CustomerOrderGroupsToUpdate { get; set; }
    }
}
