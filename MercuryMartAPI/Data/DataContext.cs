using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Data
{
    // the reason for the seemingly verbose config is so to specify to use int for primary keys rather than strings
    // AND to enable the user roles come back on a query for the user
    public class DataContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>, UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // establishing the relationship between the user, role and userRole classes
            // A USER CAN HAVE MANY ROLES AND A ROLE CAN BELONG TO MANY USERS---MANY TO MANY RELATIONSHIP
            builder.Entity<UserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });
                userRole.HasOne(ur => ur.Role).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.RoleId).IsRequired(false);
                userRole.HasOne(ur => ur.User).WithMany(r => r.UserRoles).HasForeignKey(ur => ur.UserId).IsRequired(false);
            });

            builder.Entity<Administrator>().HasOne(a => a.User).WithOne(b => b.Administrator).HasForeignKey<Administrator>(c => c.UserId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Customer>().HasOne(a => a.User).WithOne(b => b.Customer).HasForeignKey<Customer>(c => c.UserId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Product>().HasOne(a => a.Category).WithMany(b => b.Products).HasForeignKey(c => c.CategoryId).IsRequired(false);

            builder.Entity<CustomerCartItem>().HasOne(a => a.Customer).WithMany(b => b.CustomerCartItems).HasForeignKey(c => c.CustomerId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerOrder>().HasOne(a => a.Customer).WithMany(b => b.CustomerOrders).HasForeignKey(c => c.CustomerId).IsRequired(false).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerOrderGroup>().HasOne(a => a.CustomerOrder).WithMany(b => b.CustomerOrderGroups).HasForeignKey(c => c.CustomerOrderId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CustomerOrderGroupItem>().HasOne(a => a.CustomerOrderGroup).WithMany(b => b.CustomerOrderGroupItems).HasForeignKey(c => c.CustomerOrderGroupId).IsRequired(true).OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ProjectModule>().HasMany(a => a.Functionalities).WithOne(b => b.ProjectModule).HasForeignKey(c => c.ProjectModuleId).IsRequired(false);

            builder.Entity<FunctionalityRole>().HasKey(a => new { a.FunctionalityId, a.RoleId });
            builder.Entity<FunctionalityRole>().HasOne(a => a.Functionality).WithMany(b => b.FunctionalityRoles).HasForeignKey(c => c.FunctionalityId).IsRequired(true);
            builder.Entity<FunctionalityRole>().HasOne(a => a.Role).WithMany(b => b.FunctionalityRoles).HasForeignKey(c => c.RoleId).IsRequired(true);

            builder.Entity<User>().HasQueryFilter(a => !a.Deleted);
            builder.Entity<Administrator>().HasQueryFilter(a => !a.DeletedAt.HasValue);
            builder.Entity<Customer>().HasQueryFilter(a => !a.DeletedAt.HasValue);
            builder.Entity<ProjectModule>().HasQueryFilter(a => !a.DeletedAt.HasValue);
            builder.Entity<Functionality>().HasQueryFilter(a => !a.DeletedAt.HasValue);
        }

        public DbSet<ProjectModule> ProjectModule { get; set; }
        public DbSet<Functionality> Functionality { get; set; }
        public DbSet<FunctionalityRole> FunctionalityRole { get; set; }
        public DbSet<Administrator> Administrator { get; set; }
        public DbSet<Customer> Customer { get; set; }
    }
}
