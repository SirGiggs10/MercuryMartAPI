using MercuryMartAPI.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.RoleFunctionality
{
    public class RoleUserAssignmentRequest
    {
        public List<int> Users { get; set; }
        public List<int> Roles { get; set; }
    }
}