using MercuryMartAPI.Dtos.Category;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface ICategoryRepository
    {
        public Task<ReturnResponse> CreateCategory(CategoryRequest categoryRequest);
        public Task<ReturnResponse> GetCategory(UserParams userParams);
        public Task<ReturnResponse> GetCategory(int categoryId);
        public Task<ReturnResponse> UpdateCategory(int categoryId, CategoryToUpdate categoryToUpdate);
        public Task<ReturnResponse> DeleteCategory(List<int> categoriesIds);
    }
}
