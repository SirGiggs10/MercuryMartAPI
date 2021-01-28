using MercuryMartAPI.Dtos.Category;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Dtos.Product
{
    public class ProductResponse
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductDescription { get; set; }
        public double ProductPrice { get; set; }
        public int CategoryId { get; set; }
        public string ProductSerialNumber { get; set; }
        public string AttachmentLink { get; set; }
        public string AttachmentFileName { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public CategoryResponseForProduct Category { get; set; }
    }
}
