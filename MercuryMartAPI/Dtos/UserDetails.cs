using MercuryMartAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos
{
    public class UserDetails
    {
        public string Token { get; set; }
        public User User { get; set; }
        public object userProfile { get; set; }
    }
}
