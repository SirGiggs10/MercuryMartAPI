using MercuryMartAPI.Dtos.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface IDashboardRepository
    {
        public Task<ReturnResponse> GetAdministratorDashboard();
        public Task<ReturnResponse> GetCustomerDashboard();
    }
}
