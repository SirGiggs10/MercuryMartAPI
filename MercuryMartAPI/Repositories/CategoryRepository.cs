using AutoMapper;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.Category;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DataContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMapper _mapper;

        public CategoryRepository(DataContext dataContext, IGlobalRepository globalRepository, IHttpContextAccessor httpContextAccessor, IMapper mapper)
        {
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _httpContextAccessor = httpContextAccessor;
            _mapper = mapper;
        }

        public async Task<ReturnResponse> CreateCategory(CategoryRequest categoryRequest)
        {
            if(categoryRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if(string.IsNullOrWhiteSpace(categoryRequest.CategoryName))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var categoryToCreate = _mapper.Map<Category>(categoryRequest);
            var creationResult = _globalRepository.Add(categoryToCreate);
            if(!creationResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if(!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if(!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = categoryToCreate
            };
        }

        public async Task<ReturnResponse> DeleteCategory(List<int> categoriesIds)
        {
            if (categoriesIds == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var categoriesToDelete = new List<Category>();
            foreach (var t in categoriesIds)
            {
                var category = await _dataContext.Category.Where(a => a.CategoryId == t).FirstOrDefaultAsync();
                if (category == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                categoriesToDelete.Add(category);
            }

            var deletionResult = _globalRepository.Delete(categoriesToDelete);
            if(!deletionResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if(!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if(!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }
            
            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = categoriesToDelete
            };
        }

        public async Task<ReturnResponse> GetCategory(UserParams userParams)
        {
            var categories = _dataContext.Category;

            var pagedListOfCategories = await PagedList<Category>.CreateAsync(categories, userParams.PageNumber, userParams.PageSize);
            var listOfCategories = pagedListOfCategories.ToList();

            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfCategories.CurrentPage, pagedListOfCategories.PageSize, pagedListOfCategories.TotalCount, pagedListOfCategories.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = listOfCategories
            };
        }

        public async Task<ReturnResponse> GetCategory(int categoryId)
        {
            var category = await _dataContext.Category.Where(a => a.CategoryId == categoryId).FirstOrDefaultAsync();

            if (category == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = category
            };
        }

        public async Task<ReturnResponse> UpdateCategory(int categoryId, CategoryToUpdate categoryToUpdate)
        {
            if (categoryToUpdate == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if (categoryId != categoryToUpdate.CategoryId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var categoryDetails = await _dataContext.Category.Where(a => a.CategoryId == categoryId).FirstOrDefaultAsync();
            if (categoryDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var categoryToStore = _mapper.Map(categoryToUpdate, categoryDetails);
            var updateResult = _globalRepository.Update(categoryToStore);
            if (!updateResult)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if (!saveResult.HasValue)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveResult.Value)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveNoRowAffected,
                    StatusMessage = Utils.StatusMessageSaveNoRowAffected
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = categoryToStore
            };
        }
    }
}
