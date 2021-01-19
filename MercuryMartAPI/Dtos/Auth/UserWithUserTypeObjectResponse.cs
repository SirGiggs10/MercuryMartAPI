using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.Administrator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Auth
{
    public class UserWithUserTypeObjectResponse
    {
        public int Id { get; set; }
        public int UserType { get; set; } // specifies the type of user like customer or administrator
        public int UserTypeId { get; set; } // specifies the id of the user on his type table...like administrator table
        public bool EmailConfirmed { get; set; }
        public DateTimeOffset? LastLoginDateTime { get; set; }
        public DateTimeOffset? SecondToLastLoginDateTime { get; set; }
        public AdministratorResponse Administrator { get; set; }
        public CustomerResponse Customer { get; set; }
    }
}
