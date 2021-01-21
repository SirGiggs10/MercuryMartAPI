using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MercuryMartAPI.Data;
using MercuryMartAPI.Models;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Helpers;
using Microsoft.AspNetCore.Authorization;
using MercuryMartAPI.Dtos.General;
using Microsoft.Extensions.Configuration;
using MercuryMartAPI.Dtos;
using AutoMapper;
using MercuryMartAPI.Dtos.Auth;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;

namespace MercuryMartAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class CustomerManagementController : ControllerBase
    {
        private readonly DataContext _context;
        public readonly ICustomerManagementRepository _customerManagementRepository;
        public readonly IAuthRepository _authRepository;
        public readonly IConfiguration _configuration;
        public readonly IMapper _mapper;

        public CustomerManagementController(DataContext context, ICustomerManagementRepository customerManagementRepository, IAuthRepository authRepository, IConfiguration configuration, IMapper mapper)
        {
            _context = context;
            _customerManagementRepository = customerManagementRepository;
            _authRepository = authRepository;
            _configuration = configuration;
            _mapper = mapper;
        }

        /// <summary>
        /// GET ALL CUSTOMERS IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerManagement
        [RequiredFunctionalityName("GetCustomerManagements")]
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomerManagement([FromQuery] UserParams userParams)
        {
            var result = await _customerManagementRepository.GetCustomers(userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CustomerResponse>>((List<Customer>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ALL CUSTOMERS IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerManagement/5
        [RequiredFunctionalityName("GetCustomerManagement")]
        [HttpGet("{customerId}}")]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomerManagement([FromRoute] int customerId)
        {
            var result = await _customerManagementRepository.GetCustomers(customerId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<CustomerResponse>((Customer)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }

        }

        /// <summary>
        /// REGISTER A NEW CUSTOMER TO THE SYSTEM
        /// </summary>
        // POST: api/CustomerManagement/Create
        [RequiredFunctionalityName("PostCustomerManagement")]
        [HttpPost]
        public async Task<ActionResult> PostCustomerManagement([FromBody] CustomerRequest customerRequest)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerManagementRepository.CreateCustomer(customerRequest);

            if (result.StatusCode == Utils.Success)
            {
                var customer = _mapper.Map<CustomerResponse>((Customer)result.ObjectValue);
                result.ObjectValue = customer;

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
        /// UPDATE CUSTOMER INFORMATION IN THE SYSTEM
        /// </summary>
        // POST: api/CustomerManagement/Update
        [RequiredFunctionalityName("PutCustomerManagement")]
        [HttpPut("{customerId}")]
        public async Task<ActionResult> PutCustomerManagement([FromRoute] int customerId, [FromBody] CustomerToUpdate customerToUpdate)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerManagementRepository.UpdateCustomer(customerId, customerToUpdate);

            if (result.StatusCode == Utils.Success)
            {
                var customer = _mapper.Map<CustomerResponse>((Customer)result.ObjectValue);
                result.ObjectValue = customer;
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
        /// DELETE A CUSTOMER IN THE SYSTEM
        /// </summary>
        // POST: api/CustomerManagement/Delete
        [RequiredFunctionalityName("DeleteCustomerManagement")]
        [HttpPost("Delete")]
        public async Task<ActionResult> DeleteCustomerManagement([FromBody] List<int> customersIds)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync();
            var result = await _customerManagementRepository.DeleteCustomer(customersIds);

            if (result.StatusCode == Utils.Success)
            {
                var customer = _mapper.Map<CustomerResponse>((Customer)result.ObjectValue);
                result.ObjectValue = customer;
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
        /// SEARCH FOR A CUSTOMER IN THE SYSTEM
        /// </summary>
        // GET: api/CustomerManagement/Search/{searchParams}
        [RequiredFunctionalityName("SearchCustomerFromManagement")]
        [HttpGet("Search/{searchParams}")]
        public async Task<ActionResult> GetSearchCustomer([FromRoute] string searchParams, [FromQuery] UserParams userParams)
        {
            var result = await _customerManagementRepository.SearchCustomer(searchParams, userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<CustomerResponse>>((List<Customer>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}