using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos
{
    public class UserForLoginDto
    {
        public string EmailAddress { get; set; }
        public string Password { get; set; }
    }
}
