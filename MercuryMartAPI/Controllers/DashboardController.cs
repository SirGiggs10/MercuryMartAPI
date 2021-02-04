using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;
using MercuryMartAPI.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardRepository _dashboardRepository;

        public DashboardController(IDashboardRepository dashboardRepository)
        {
            _dashboardRepository = dashboardRepository;
        }

        /// <summary>
        /// GET DASHBOARD FOR ADMINISTRATOR
        /// </summary>
        // GET: api/Dashboard/Administrator
        [RequiredFunctionalityName("GetAdministratorDashboard")]
        [HttpGet("Administrator")]
        public async Task<ActionResult<ReturnResponse>> GetAdministratorDashboard()
        {
            var result = await _dashboardRepository.GetAdministratorDashboard();

            if (result.StatusCode == Utils.Success)
            {
                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
        /*
        /// <summary>
        /// GET DASHBOARD FOR CUSTOMER
        /// </summary>
        // GET: api/Dashboard/Customer
        [RequiredFunctionalityName("GetCustomerDashboard")]
        [HttpGet("Customer")]
        public async Task<ActionResult<ReturnResponse>> GetCustomerDashboard()
        {
            var result = await _dashboardRepository.GetCustomerDashboard();

            if (result.StatusCode == Utils.Success)
            {
                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }*/
    }
}
