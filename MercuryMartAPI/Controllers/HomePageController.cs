using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
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
    public class HomePageController : ControllerBase
    {
        private readonly IHomePageRepository _homePageRepository;

        public HomePageController(IHomePageRepository homePageRepository)
        {
            _homePageRepository = homePageRepository;
        }

        /// <summary>
        /// GET ALL CATEGORIES IN THE SYSTEM
        /// </summary>
        // GET: api/Categories
        [AllowAnonymous]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetHomePage()
        {
            var result = await _homePageRepository.GetHomePage();

            if (result.StatusCode == Utils.Success)
            {
                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}
