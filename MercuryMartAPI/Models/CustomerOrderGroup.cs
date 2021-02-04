using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Models
{
    public class CustomerOrderGroup
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CustomerOrderGroupId { get; set; }
        public int CustomerOrderId { get; set; }
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public int QuantityOrdered { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual CustomerOrder CustomerOrder { get; set; }
        public virtual List<CustomerOrderGroupItem> CustomerOrderGroupItems { get; set; }
    }
}
