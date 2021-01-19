using MercuryMartAPI.Dtos;
using MercuryMartAPI.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.Auth;
using MercuryMartAPI.Dtos.RoleFunctionality;
using MercuryMartAPI.Dtos.Administrator;

namespace MercuryMartAPI.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Customer, CustomerResponse>();

            CreateMap<Administrator, AdministratorResponse>();
            
            CreateMap<User, UserDetails>();
            CreateMap<UserDetails, UserLoginResponse>();
            CreateMap<User, UserToReturn>();
            CreateMap<User, UserWithUserTypeObjectResponse>();
            CreateMap<User, UserToReturnForLogin>();
            CreateMap<UserDetails, UserLoginResponseForLogin>();

            CreateMap<FunctionalityRequest, Functionality>();
            CreateMap<Functionality, FunctionalityResponse>();

            CreateMap<ProjectModuleRequest, ProjectModule>();
            CreateMap<ProjectModule, ProjectModuleResponse>();

            CreateMap<RoleRequest, Role>();
            CreateMap<Role, RoleResponse>();
            CreateMap<RoleResponse, Role>().ForMember(a => a.Id, b => b.Ignore()).ForMember(a1 => a1.DeletedAt, b1 => b1.Ignore()).ForMember(a2 => a2.CreatedAt, b2 => b2.Ignore());
            CreateMap<Role, RoleResponseForLogin>();
            CreateMap<UserRole, UserRoleResponseForLogin>();

            CreateMap<CustomerCartItemRequest, CustomerCartItem>();
            CreateMap<CustomerCartItem, CustomerCartItemResponse>();
        }
    }
}
