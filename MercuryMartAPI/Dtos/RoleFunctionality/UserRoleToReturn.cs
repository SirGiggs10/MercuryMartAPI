﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.RoleFunctionality
{
    public class UserRoleToReturn
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public RoleResponse Role { get; set; }
    }
}
