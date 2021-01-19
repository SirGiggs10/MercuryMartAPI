using MercuryMartAPI.Data;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MercuryMartAPI.Models;
using MercuryMartAPI.Dtos.General;

namespace MercuryMartAPI.Helpers.AuthorizationMiddleware
{
    public class FunctionalityNameHandler : AuthorizationHandler<FunctionalityNameRequirement>
    {
        private readonly DataContext _dataContext;

        public FunctionalityNameHandler(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, FunctionalityNameRequirement requirement)
        {
            if (!context.User.HasClaim(c => c.Type == ClaimTypes.Role))
            {
                return Task.FromResult(0);
            }

            var userType = context.User.Claims.FirstOrDefault(a => a.Type == Utils.ClaimType_UserType);
            var userId = context.User.Claims.FirstOrDefault(a => a.Type == ClaimTypes.Name);
            if (userType == null || userId == null)
            {
                return Task.FromResult(0);
            }

            // check if any of the user's roles has the the functionality name mapped to it.
            var funcName = requirement.FuncName;
            var roles = context.User.FindAll(c => c.Type == ClaimTypes.Role).Select(c => c.Value).ToList();
            var funcRole = _dataContext.FunctionalityRole.Where(a => a.FunctionalityName == funcName).Select(b => b.RoleName).ToList();

            foreach (var role in roles)
            {
                if (funcRole.Contains(role))
                {
                    context.Succeed(requirement);
                    return Task.FromResult(0);
                }
            }

            //context.Fail();   //CHECK LATER
            return Task.FromResult(0);
        }
    }
}
