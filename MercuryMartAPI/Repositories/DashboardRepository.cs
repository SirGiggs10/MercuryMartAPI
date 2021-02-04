using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.Dashboard;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class DashboardRepository : IDashboardRepository
    {
        private readonly DataContext _dataContext;

        public DashboardRepository(DataContext dataContext)
        {
            _dataContext = dataContext;
        }

        public async Task<ReturnResponse> GetAdministratorDashboard()
        {
            var administratorDashboard = new AdministratorDashboardResponse();
            administratorDashboard.AdministratorTotalCount = await _dataContext.Administrator.CountAsync();
            administratorDashboard.CustomerTotalCount = await _dataContext.Customer.CountAsync();
            administratorDashboard.CategoryTotalCount = await _dataContext.Category.CountAsync();
            administratorDashboard.ProductTotalCount = await _dataContext.Product.Where(a => !a.AssignedTo.HasValue).CountAsync();
            administratorDashboard.SuccesfulOrderTotalCount = await _dataContext.CustomerOrder.Where(a => a.OrderStatus == Utils.CurrentOrderNumberStatus_Completed).CountAsync();
            administratorDashboard.SalesTotalPrice = await _dataContext.CustomerOrder.Where(a => a.OrderStatus == Utils.CurrentOrderNumberStatus_Completed).Include(b => b.CustomerOrderGroups).ThenInclude(c => c.CustomerOrderGroupItems).SelectMany(d => d.CustomerOrderGroups).SelectMany(e => e.CustomerOrderGroupItems).Include(f => f.Product).Select(g => g.Product.ProductPrice).SumAsync();
            administratorDashboard.SalesTotalPriceTemp = await _dataContext.Product.Where(a => a.AssignedTo.HasValue).SumAsync(a => a.ProductPrice);
            
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = administratorDashboard
            };
        }

        public async Task<ReturnResponse> GetCustomerDashboard()
        {
            throw new NotImplementedException();
        }
    }
}
