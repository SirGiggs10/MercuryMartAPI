using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.RoleFunctionality
{
    public class FunctionalityRoleResponse
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; }
        public int FunctionalityId { get; set; }
        public string FunctionalityName { get; set; }
    }
}
