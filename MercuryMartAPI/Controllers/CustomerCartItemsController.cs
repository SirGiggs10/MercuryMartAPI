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
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Helpers;
using AutoMapper;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerCartItemsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ICustomerCartItemRepository _customerCartItemRepository;
        private readonly IMapper _mapper;

        public CustomerCartItemsController(DataContext dataContext, ICustomerCartItemRepository customerCartItemRepository, IMapper mapper)
        {
            _dataContext = dataContext;
            _customerCartItemRepository = customerCartItemRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// GET ALL CART ITEMS FOR A CUSTOMER IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerCartItems
        [RequiredFunctionalityName("GetCustomerCartItem")]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetCustomerCartItem()
        {
            var result = await _customerCartItemRepository.GetCustomerCartItem();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CustomerCartItemResponse>>((List<CustomerCartItem>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// ADD AN ITEM TO CART FOR A CUSTOMER IN THE SYSTEM
        /// </summary>
        // POST: api/CustomerCartItems
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostCustomerCartItem")]
        [HttpPost]
        public async Task<ActionResult<ReturnResponse>> PostCustomerCartItem([FromBody] CustomerCartItemRequest customerCartItemRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _customerCartItemRepository.CreateCustomerCartItem(customerCartItemRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CustomerCartItemResponse>((CustomerCartItem)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE AN ITEM FROM THE CART FOR A CUSTOMER IN THE SYSTEM
        /// </summary>
        // DELETE: api/CustomerCartItems/5
        [RequiredFunctionalityName("DeleteCustomerCartItem")]
        [HttpDelete("{customerCartItemId}")]
        public async Task<ActionResult<ReturnResponse>> DeleteCustomerCartItem(int customerCartItemId)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _customerCartItemRepository.DeleteCustomerCartItem(customerCartItemId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CustomerCartItemResponse>((CustomerCartItem)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE ALL CART ITEMS FOR A CUSTOMER IN THE SYSTEM
        /// </summary>
        // DELETE: api/CustomerCartItems/5
        [RequiredFunctionalityName("DeleteCustomerCartItems")]
        [HttpDelete]
        public async Task<ActionResult<ReturnResponse>> DeleteCustomerCartItem()
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _customerCartItemRepository.DeleteCustomerCartItem();

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CustomerCartItemResponse>>((List<CustomerCartItem>)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
