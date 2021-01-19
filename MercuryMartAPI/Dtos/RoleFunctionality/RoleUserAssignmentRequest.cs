using MercuryMartAPI.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.RoleFunctionality
{
    public class RoleUserAssignmentRequest
    {
        public List<UserToReturn> Users { get; set; }
        public List<RoleResponse> Roles { get; set; }
    }
}