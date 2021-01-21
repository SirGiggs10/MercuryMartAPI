using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.RoleFunctionality
{
    public class FunctionalityRoleAssignmentRequest
    {
        public int RoleId { get; set; }
        public List<int> FunctionalityIds { get; set; }
    }
}