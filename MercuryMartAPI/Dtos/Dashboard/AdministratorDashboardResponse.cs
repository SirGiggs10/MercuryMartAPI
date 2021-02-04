using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Dashboard
{
    public class AdministratorDashboardResponse
    {
        public int AdministratorTotalCount { get; set; }
        public int CustomerTotalCount { get; set; }
        public int CategoryTotalCount { get; set; }
        public int ProductTotalCount { get; set; }
        public int SuccesfulOrderTotalCount { get; set; }
        public double SalesTotalPrice { get; set; }
        public double SalesTotalPriceTemp { get; set; }
    }
}
