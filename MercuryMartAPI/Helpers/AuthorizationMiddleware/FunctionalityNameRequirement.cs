using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Helpers.AuthorizationMiddleware
{
    public class FunctionalityNameRequirement : IAuthorizationRequirement
    {
        public FunctionalityNameRequirement(string functionalityName)
        {
            FuncName = functionalityName;
        }

        public string FuncName { get; private set; }
    }
}
