using MercuryMartAPI.Dtos.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.RoleFunctionality
{
    public class UserRoleResponse
    {
        public int Id { get; set; }
        public UserToReturn User { get; set; }
        public RoleResponse Role { get; set; }
    }
}
