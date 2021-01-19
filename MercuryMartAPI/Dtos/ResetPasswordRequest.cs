using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos
{
    public class ResetPasswordRequest
    {
        public string EmailAddress { get; set; }
        public string NewPassword { get; set; }
        public string PasswordResetLinkToken { get; set; }
    }
}
