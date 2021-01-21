using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Administrator
{
    public class AdministratorRequest
    {
        public string FullName { get; set; }
        public string EmailAddress { get; set; }
        public int RoleId { get; set; }
    }
}
