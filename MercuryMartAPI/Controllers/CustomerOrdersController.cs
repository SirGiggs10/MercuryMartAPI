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
using MercuryMartAPI.Interfaces;
using AutoMapper;
using MercuryMartAPI.Dtos.CustomerOrder;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CustomerOrdersController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly ICustomerOrderRepository _customerOrderRepository;
        private readonly IMapper _mapper;

        public CustomerOrdersController(DataContext dataContext, ICustomerOrderRepository customerOrderRepository, IMapper mapper)
        {
            _dataContext = dataContext;
            _customerOrderRepository = customerOrderRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// GET ALL CUSTOMER ORDERS BY ONE CUSTOMER IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerOrders
        [RequiredFunctionalityName("GetCustomerOrders")]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetCustomerOrder([FromQuery] UserParams userParams)
        {
            var result = await _customerOrderRepository.GetCustomerOrder(userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CustomerOrderResponse>>((List<CustomerOrder>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ONE CUSTOMER ORDER IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerOrders/5
        [RequiredFunctionalityName("GetCustomerOrder")]
        [HttpGet("{customerOrderId}")]
        public async Task<ActionResult<ReturnResponse>> GetCustomerOrder(int customerOrderId)
        {
            var result = await _customerOrderRepository.GetCustomerOrder(customerOrderId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CustomerOrderResponse>((CustomerOrder)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
        /*
        // PUT: api/CustomerOrders/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCustomerOrder(int id, CustomerOrder customerOrder)
        {
            
        }
        */
        /// <summary>
        /// CREATE A CUSTOMER ORDER IN THE SYSTEM
        /// </summary>
        // POST: api/CustomerOrders
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostCustomerOrder")]
        [HttpPost]
        public async Task<ActionResult<ReturnResponse>> PostCustomerOrder([FromBody] CustomerOrderRequest customerOrderRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _customerOrderRepository.CreateCustomerOrder(customerOrderRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CustomerOrderResponse>((CustomerOrder)result.ObjectValue);
                await dbTransaction.CommitAsync();

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                await dbTransaction.RollbackAsync();

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
        /*
        // DELETE: api/CustomerOrders/5
        [RequiredFunctionalityName("DeleteCustomerOrder")]
        [HttpDelete("{id}")]
        public async Task<ActionResult<CustomerOrder>> DeleteCustomerOrder(int id)
        {
            
        }
        */
    }
}
