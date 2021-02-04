using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.HomePage
{
    public class ProductCard
    {
        public int CategoryId { get; set; }
        public string ProductName { get; set; }
        public int QuantityInStock { get; set; }
    }
}
