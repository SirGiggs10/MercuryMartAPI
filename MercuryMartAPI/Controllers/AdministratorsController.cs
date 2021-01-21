using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MercuryMartAPI.Data;
using MercuryMartAPI.Models;
using AutoMapper;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Dtos.Administrator;
using Microsoft.AspNetCore.Authorization;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AdministratorsController : ControllerBase
    {
        private readonly DataContext _dataContext;
        private readonly IMapper _mapper;
        private readonly IAdministratorRepository _administratorRepository;

        public AdministratorsController(DataContext dataContext, IMapper mapper, IAdministratorRepository administratorRepository)
        {
            _dataContext = dataContext;
            _mapper = mapper;
            _administratorRepository = administratorRepository;
        }

        /// <summary>
        /// GET ALL ADMINISTRATORS IN THE SYSTEM
        /// </summary>
        // GET: api/Administrators
        [RequiredFunctionalityName("GetAdministrators")]
        [HttpGet]
        public async Task<ActionResult<ReturnResponse>> GetAdministrator([FromQuery] UserParams userParams)
        {
            var result = await _administratorRepository.GetAdministrators(userParams);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<AdministratorResponse>>((List<Administrator>)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ONE ADMINISTRATOR IN THE SYSTEM
        /// </summary>
        // GET: api/Administrators/5
        [RequiredFunctionalityName("GetAdministrator")]
        [HttpGet("{administratorId}")]
        public async Task<ActionResult<ReturnResponse>> GetAdministrator([FromRoute] int administratorId)
        {
            var result = await _administratorRepository.GetAdministrators(administratorId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<AdministratorResponse>((Administrator)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result.ObjectValue);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// UPDATE ADMINISTRATOR INFORMATION IN THE SYSTEM
        /// </summary>
        // PUT: api/Administrators/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PutAdministrator")]
        [HttpPut("{administratorId}")]
        public async Task<ActionResult<ReturnResponse>> PutAdministrator([FromRoute] int administratorId, [FromBody] AdministratorToUpdate administratorToUpdate)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _administratorRepository.UpdateAdministrator(administratorId, administratorToUpdate);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<AdministratorResponse>((Administrator)result.ObjectValue);
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
        /// REGISTER A NEW ADMINISTRATOR TO THE SYSTEM
        /// </summary>
        // POST: api/Administrators
        // To protect from overposting attacks, enable the specific properties you want to bind to, for
        // more details, see https://go.microsoft.com/fwlink/?linkid=2123754.
        [RequiredFunctionalityName("PostAdministrator")]
        [HttpPost]
        public async Task<ActionResult<ReturnResponse>> PostAdministrator([FromBody] AdministratorRequest administratorRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _administratorRepository.CreateAdministrator(administratorRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<AdministratorResponse>((Administrator)result.ObjectValue);
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
        /// DELETE ADMINISTRATORS IN THE SYSTEM
        /// </summary>
        // POST: api/Administrators/Delete
        [RequiredFunctionalityName("DeleteAdministrator")]
        [HttpPost("Delete")]
        public async Task<ActionResult<ReturnResponse>> DeleteAdministrator([FromBody] List<int> administratorsIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _administratorRepository.DeleteAdministrator(administratorsIds);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<AdministratorResponse>>((List<Administrator>)result.ObjectValue);
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
