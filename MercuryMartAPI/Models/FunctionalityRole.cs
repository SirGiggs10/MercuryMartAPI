using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Models
{
    public class FunctionalityRole
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? DeletedAt { get; set; }

        public virtual Functionality Functionality { get; set; }
        public virtual Role Role { get; set; }
    }
}
