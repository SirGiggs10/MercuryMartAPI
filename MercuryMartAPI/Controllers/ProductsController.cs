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
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Dtos.Product;
using MercuryMartAPI.Interfaces;
using AutoMapper;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public ProductsController(DataContext dataContext, IProductRepository productRepository, IMapper mapper)
        {
            _dataContext = dataContext;
            _productRepository = productRepository;
            _mapper = mapper;
        }

        // GET: api/Products
        [RequiredFunctionalityName("GetProducts")]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetProduct([FromQuery] UserParams userParams)
        {
            var result = await _productRepository.GetProduct(userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProductResponse>>((List<Product>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // GET: api/Products/5
        [RequiredFunctionalityName("GetProduct")]
        [HttpGet("{productId}")]
        public async Task<ActionResult<ReturnResponse>> GetProduct([FromRoute] int productId)
        {
            var result = await _productRepository.GetProduct(productId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<ProductResponse>((Product)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // GET: api/Products/5
        [RequiredFunctionalityName("GetProductByCategory")]
        [HttpGet("Category/{categoryId}")]
        public async Task<ActionResult<ReturnResponse>> GetProductByCategory([FromRoute] int categoryId, [FromQuery] UserParams userParams)
        {
            var result = await _productRepository.GetProductByCategory(categoryId, userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProductResponse>>((List<Product>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // PUT: api/Products/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutProduct")]
        [HttpPut("{productId}")]
        public async Task<ActionResult<ReturnResponse>> PutProduct([FromRoute] int productId, [FromForm] ProductToUpdate productToUpdate)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _productRepository.UpdateProduct(productId, productToUpdate);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<ProductResponse>((Product)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // POST: api/Products
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostProduct")]
        [HttpPost]
        public async Task<ActionResult<ReturnResponse>> PostProduct([FromForm] ProductRequest productRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _productRepository.CreateProduct(productRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<ProductResponse>((Product)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        // DELETE: api/Products/5
        [RequiredFunctionalityName("DeleteProduct")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ReturnResponse>> DeleteProduct([FromBody] List<int> productsIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _productRepository.DeleteProduct(productsIds);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProductResponse>>((List<Product>)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
