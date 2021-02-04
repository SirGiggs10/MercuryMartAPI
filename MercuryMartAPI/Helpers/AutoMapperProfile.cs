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
using MercuryMartAPI.Dtos.Category;
using MercuryMartAPI.Dtos.Product;
using MercuryMartAPI.Dtos.CustomerOrder;
using MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup;
using MercuryMartAPI.Dtos.CustomerOrder.CustomerOrderGroup.CustomerOrderGroupItem;

namespace MercuryMartAPI.Helpers
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CustomerRequest, Customer>();
            CreateMap<Customer, CustomerResponse>();
            CreateMap<CustomerToUpdate, Customer>();

            CreateMap<AdministratorRequest, Administrator>();
            CreateMap<Administrator, AdministratorResponse>();
            CreateMap<AdministratorToUpdate, Administrator>();
            
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

            CreateMap<FunctionalityRole, FunctionalityRoleResponse>();

            CreateMap<CategoryRequest, Category>();
            CreateMap<Category, CategoryResponse>();
            CreateMap<CategoryToUpdate, Category>();
            CreateMap<Category, CategoryResponseForProduct>();

            CreateMap<ProductRequest, Product>();
            CreateMap<Product, ProductResponse>();
            CreateMap<ProductToUpdate, Product>();

            CreateMap<CustomerOrderRequest, CustomerOrder>();
            CreateMap<CustomerOrder, CustomerOrderResponse>();
            CreateMap<CustomerOrderToUpdate, CustomerOrder>();
            CreateMap<CustomerOrderGroupRequest, CustomerOrderGroup>();
            CreateMap<CustomerOrderGroup, CustomerOrderGroupResponse>();
            CreateMap<CustomerOrderGroupToUpdate, CustomerOrderGroup>();
            CreateMap<CustomerOrderGroupItemRequest, CustomerOrderGroupItem>();
            CreateMap<CustomerOrderGroupItem, CustomerOrderGroupItemResponse>();
            CreateMap<CustomerOrderGroupItemToUpdate, CustomerOrderGroupItem>();
        }
    }
}
