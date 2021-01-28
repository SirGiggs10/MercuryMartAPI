using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Dtos.Product;
using MercuryMartAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface IProductRepository
    {
        public Task<ReturnResponse> CreateProduct(ProductRequest productRequest);
        public Task<ReturnResponse> GetProduct(UserParams userParams);
        public Task<ReturnResponse> GetProductByCategory(int categoryId, UserParams userParams);
        public Task<ReturnResponse> GetProduct(int productId);
        public Task<ReturnResponse> UpdateProduct(int productId, ProductToUpdate productToUpdate);
        public Task<ReturnResponse> DeleteProduct(List<int> productsIds);
    }
}
