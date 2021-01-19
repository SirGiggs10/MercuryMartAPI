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
        // GET: api/CustomerManagement/{custom
        [RequiredFunctionalityName("GetCustomersFromManagement")]
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
        /// REGISTER A NEW CUSTOMER TO THE SYSTEM
        /// </summary>
        // POST: api/CustomerManagement/Create
        [RequiredFunctionalityName("PostCreateCustomerFromManagement")]
        [HttpPost("Create/")]
        public async Task<ActionResult> PostCreateCustomer(CustomerRequest customerRequest)
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