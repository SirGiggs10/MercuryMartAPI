using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.General
{
    public class UserEmailRequest
    {
        public string EmailConfirmationLinkToken { get; set; }
    }
}
