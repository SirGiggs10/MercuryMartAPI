using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Models
{
    public class CustomerOrder
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        [Key]
        public int CustomerOrderId { get; set; }
        public int CustomerId { get; set; }
        public string OrderName { get; set; }
        public int OrderStatus { get; set; }
        public int DeliveryStatus { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual Customer Customer { get; set; }
        public virtual List<CustomerOrderGroup> CustomerOrderGroups { get; set; }
    }
}
