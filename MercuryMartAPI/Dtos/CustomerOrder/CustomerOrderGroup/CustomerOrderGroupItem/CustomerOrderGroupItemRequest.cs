using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup.CustomerOrderGroupItem
{
    public class CustomerOrderGroupItemRequest
    {
        public int CustomerOrderGroupId { get; set; }
        public int ProductId { get; set; }
    }
}
