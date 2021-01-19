using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Auth
{
    public class RegisterResponse
    {
        public UserLoginRequest UserLoginDetails { get; set; }
        public object ObjectValue { get; set; }
    }
}
