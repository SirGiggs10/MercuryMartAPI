using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Administrator
{
    public class AdministratorResponse
    {
        public int AdministratorId { get; set; }
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public int UserId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }
}
