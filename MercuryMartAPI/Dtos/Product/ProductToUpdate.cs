using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Product
{
    public class ProductToUpdate
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public double ProductPrice { get; set; }
        public double ProductCost { get; set; }
        public int CategoryId { get; set; }
        public string ProductSerialNumber { get; set; }
        public IFormFile AttachmentFile { get; set; }
    }
}
