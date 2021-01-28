using AutoMapper;
using CloudinaryDotNet.Actions;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Dtos.Product;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly DataContext _dataContext;
        private readonly IGlobalRepository _globalRepository;
        private readonly IMapper _mapper;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICloudinaryRepository _cloudinaryRepository;

        public ProductRepository(DataContext dataContext, IGlobalRepository globalRepository, IMapper mapper, IHttpContextAccessor httpContextAccessor, ICloudinaryRepository cloudinaryRepository)
        {
            _dataContext = dataContext;
            _globalRepository = globalRepository;
            _mapper = mapper;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryRepository = cloudinaryRepository;
        }

        public async Task<ReturnResponse> CreateProduct(ProductRequest productRequest)
        {
            if (productRequest == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if (string.IsNullOrWhiteSpace(productRequest.ProductName) || string.IsNullOrWhiteSpace(productRequest.ProductSerialNumber) || (productRequest.ProductPrice == Utils.Zero_Price))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var category = await _globalRepository.Get<Category>(productRequest.CategoryId);
            if(category == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var productToCreate = _mapper.Map<Product>(productRequest);

            //FINALLY UPLOAD THE IMAGE THAT CAME WITH THE PRODUCT IF ANY
            var cloudinaryResult = _cloudinaryRepository.UploadFilesToCloudinary(productRequest.AttachmentFile);
            if (cloudinaryResult.StatusCode != Utils.Success)
            {
                if (cloudinaryResult.StatusCode != Utils.ObjectNull)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.CloudinaryFileUploadError,
                        StatusMessage = Utils.StatusMessageCloudinaryFileUploadError
                    };
                }
            }
            else
            {
                var cloudinaryUploadResult = (RawUploadResult)cloudinaryResult.ObjectValue;
                productToCreate.AttachmentPublicId = cloudinaryUploadResult.PublicId;
                productToCreate.AttachmentLink = cloudinaryUploadResult.SecureUrl.ToString().Split(cloudinaryUploadResult.PublicId)[0] + cloudinaryUploadResult.PublicId + Path.GetExtension(productRequest.AttachmentFile.FileName);
                productToCreate.AttachmentFileName = productRequest.AttachmentFile.FileName;
            }

            var creationResult = _globalRepository.Add(productToCreate);
            if (!creationResult)
            {
                if(cloudinaryResult.StatusCode == Utils.Success)
                {
                    var cloudinaryDeleteResult = _cloudinaryRepository.DeleteFilesFromCloudinary(new List<string>()
                    {
                        productToCreate.AttachmentPublicId
                    });

                    if(cloudinaryDeleteResult.StatusCode != Utils.Success)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.CloudinaryFileDeleteError,
                            StatusMessage = Utils.StatusMessageCloudinaryFileDeleteError
                        };
                    }
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }

            var saveResult = await _globalRepository.SaveAll();
            if (!saveResult.HasValue)
            {
                if (cloudinaryResult.StatusCode == Utils.Success)
                {
                    var cloudinaryDeleteResult = _cloudinaryRepository.DeleteFilesFromCloudinary(new List<string>()
                    {
                        productToCreate.AttachmentPublicId
                    });

                    if (cloudinaryDeleteResult.StatusCode != Utils.Success)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.CloudinaryFileDeleteError,
                            StatusMessage = Utils.StatusMessageCloudinaryFileDeleteError
                        };
                    }
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.SaveError,
                    StatusMessage = Utils.StatusMessageSaveError
                };
            }

            if (!saveResult.Value)
            {
                if (cloudinaryResult.StatusCode == Utils.Success)
                {
                    var cloudinaryDeleteResult = _cloudinaryRepository.DeleteFilesFromCloudinary(new List<string>()
                    {
                        productToCreate.AttachmentPublicId
                    });

                    if (cloudinaryDeleteResult.StatusCode != Utils.Success)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.CloudinaryFileDeleteError,
                            StatusMessage = Utils.StatusMessageCloudinaryFileDeleteError
                        };
                    }
                }

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
                ObjectValue = productToCreate
            };
        }

        public async Task<ReturnResponse> DeleteProduct(List<int> productsIds)
        {
            if (productsIds == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var productsToDelete = new List<Product>();
            var productImagesPublicIdsToDelete = new List<string>();
            foreach (var t in productsIds)
            {
                var product = await _dataContext.Product.Where(a => a.ProductId == t).FirstOrDefaultAsync();
                if (product == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }

                productsToDelete.Add(product);
            }

            productImagesPublicIdsToDelete = productsToDelete.Select(a => a.AttachmentPublicId).ToList();

            var deletionResult = _globalRepository.Delete(productsToDelete);
            if (!deletionResult)
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

            //DELETE THE PRODUCT IMAGES FROM CLOUDINARY
            var cloudinaryDeleteResult = _cloudinaryRepository.DeleteFilesFromCloudinary(productImagesPublicIdsToDelete);
            if (cloudinaryDeleteResult.StatusCode != Utils.Success)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.CloudinaryFileDeleteError,
                    StatusMessage = Utils.StatusMessageCloudinaryFileDeleteError
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = productsToDelete
            };
        }

        public async Task<ReturnResponse> GetProduct(UserParams userParams)
        {
            var products = _dataContext.Product.Include(a => a.Category);

            var pagedListOfProducts = await PagedList<Product>.CreateAsync(products, userParams.PageNumber, userParams.PageSize);
            var listOfProducts = pagedListOfProducts.ToList();

            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfProducts.CurrentPage, pagedListOfProducts.PageSize, pagedListOfProducts.TotalCount, pagedListOfProducts.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = listOfProducts
            };
        }

        public async Task<ReturnResponse> GetProduct(int productId)
        {
            var product = await _dataContext.Product.Where(a => a.ProductId == productId).Include(b => b.Category).FirstOrDefaultAsync();

            if (product == null)
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
                ObjectValue = product
            };
        }

        public async Task<ReturnResponse> GetProductByCategory(int categoryId, UserParams userParams)
        {
            var category = await _globalRepository.Get<Category>(categoryId);
            if(category == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var products = _dataContext.Product.Where(a => a.CategoryId == categoryId).Include(b => b.Category);

            var pagedListOfProducts = await PagedList<Product>.CreateAsync(products, userParams.PageNumber, userParams.PageSize);
            var listOfProducts = pagedListOfProducts.ToList();
            _httpContextAccessor.HttpContext.Response.AddPagination(pagedListOfProducts.CurrentPage, pagedListOfProducts.PageSize, pagedListOfProducts.TotalCount, pagedListOfProducts.TotalPages);

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = products
            };
        }

        public async Task<ReturnResponse> UpdateProduct(int productId, ProductToUpdate productToUpdate)
        {
            if (productToUpdate == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if (productId != productToUpdate.ProductId)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }

            var productDetails = await _dataContext.Product.Where(a => a.ProductId == productId).FirstOrDefaultAsync();
            if (productDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var category = await _globalRepository.Get<Category>(productToUpdate.CategoryId);
            if(category == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var productToStore = _mapper.Map(productToUpdate, productDetails);

            //FINALLY UPLOAD THE IMAGE THAT CAME WITH THE PRODUCT IF ANY
            var cloudinaryResult = _cloudinaryRepository.UploadFilesToCloudinary(productToUpdate.AttachmentFile);
            if (cloudinaryResult.StatusCode != Utils.Success)
            {
                if (cloudinaryResult.StatusCode != Utils.ObjectNull)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.CloudinaryFileUploadError,
                        StatusMessage = Utils.StatusMessageCloudinaryFileUploadError
                    };
                }
            }
            else
            {
                var cloudinaryUploadResult = (RawUploadResult)cloudinaryResult.ObjectValue;

                //DELETE THE OLD ATTACHED FILE
                var cloudinaryDeleteResult = _cloudinaryRepository.DeleteFilesFromCloudinary(new List<string>()
                {
                    productToStore.AttachmentPublicId
                });

                if (cloudinaryDeleteResult.StatusCode != Utils.Success)
                {
                    //IF THE OLD ATTACHED FILE IN THE CLOUDINARY DIDNT DELETE SUCCESSFULLY THEN REMOVE THE NEWLY ADDED FILE
                    var cloudinaryDelResult = _cloudinaryRepository.DeleteFilesFromCloudinary(new List<string>()
                    {
                        cloudinaryUploadResult.PublicId
                    });

                    if (cloudinaryDelResult.StatusCode != Utils.Success)
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.CloudinaryFileDeleteError,
                            StatusMessage = Utils.StatusMessageCloudinaryFileDeleteError
                        };
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };
                    }
                }
                else
                {
                    productToStore.AttachmentPublicId = cloudinaryUploadResult.PublicId;
                    productToStore.AttachmentLink = cloudinaryUploadResult.SecureUrl.ToString().Split(cloudinaryUploadResult.PublicId)[0] + cloudinaryUploadResult.PublicId + Path.GetExtension(productToUpdate.AttachmentFile.FileName);
                    productToStore.AttachmentFileName = productToUpdate.AttachmentFile.FileName;
                }
            }

            var updateResult = _globalRepository.Update(productToStore);
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
                ObjectValue = productToStore
            };
        }
    }
}
