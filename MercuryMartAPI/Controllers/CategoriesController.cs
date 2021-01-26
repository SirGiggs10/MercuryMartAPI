using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MercuryMartAPI.Data;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Dtos.General;
using AutoMapper;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Dtos.Category;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public CategoriesController(DataContext dataContext, IMapper mapper, ICategoryRepository categoryRepository)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        /// <summary>
        /// GET ALL CATEGORIES IN THE SYSTEM
        /// </summary>
        // GET: api/Categories
        [RequiredFunctionalityName("GetCategories")]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetCategory([FromQuery] UserParams userParams)
        {
            var result = await _categoryRepository.GetCategory(userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CategoryResponse>>((List<Category>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ONE CATEGORY IN THE SYSTEM
        /// </summary>
        // GET: api/Categories/5
        [RequiredFunctionalityName("GetCategory")]
        [HttpGet("{categoryId}")]
        public async Task<ActionResult<ReturnResponse>> GetCategory([FromRoute] int categoryId)
        {
            var result = await _categoryRepository.GetCategory(categoryId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CategoryResponse>((Category)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// UPDATE CATEGORY INFORMATION IN THE SYSTEM
        /// </summary>
        // PUT: api/Categories/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutCategory")]
        [HttpPut("{categoryId}")]
        public async Task<ActionResult<ReturnResponse>> PutCategory([FromRoute] int categoryId, [FromBody] CategoryToUpdate categoryToUpdate)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _categoryRepository.UpdateCategory(categoryId, categoryToUpdate);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CategoryResponse>((Category)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// ADD A NEW CATEGORY TO THE SYSTEM
        /// </summary>
        // POST: api/Categorys
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostCategory")]
        [HttpPost]
        public async Task<ActionResult<ReturnResponse>> PostCategory([FromBody] CategoryRequest categoryRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _categoryRepository.CreateCategory(categoryRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CategoryResponse>((Category)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE CATEGORIES IN THE SYSTEM
        /// </summary>
        // POST: api/Categorys/Delete
        [RequiredFunctionalityName("DeleteCategory")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ReturnResponse>> DeleteCategory([FromBody] List<int> categoriesIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _categoryRepository.DeleteCategory(categoriesIds);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CategoryResponse>>((List<Category>)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
