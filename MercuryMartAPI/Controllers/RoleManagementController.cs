//LATEST
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Dtos.RoleFunctionality;
using MercuryMartAPI.Helpers;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class RoleManagementController : ControllerBase
    {
        private readonly IRoleManagementRepository _roleManagementRepository;
        private readonly IMapper _mapper;
        private readonly DataContext _dataContext;

        public RoleManagementController(IRoleManagementRepository roleManagementRepository, IMapper mapper, DataContext dataContext)
        {
            _roleManagementRepository = roleManagementRepository;
            _mapper = mapper;
            _dataContext = dataContext;
        }

        /// <summary>
        /// CREATE ROLE IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostRoles")]
        [HttpPost("Roles/Create")]
        public async Task<ActionResult> PostRoles([FromBody] List<RoleRequest> roles)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.CreateRoles(_mapper.Map<List<Role>>(roles));

            if (result.StatusCode == Utils.Success)
            {
                var listOfRoles = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
                result.ObjectValue = listOfRoles;
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
        /// GET ALL ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetRoles")]
        [HttpGet("Roles")]
        public async Task<ActionResult> GetRoles([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetRoles(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var rolesToReturn = new List<RoleResponse>();
                var roles = (PagedList<Role>)result.ObjectValue;
                var roleList = roles.ToList();
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(roleList);
                Response.AddPagination(roles.CurrentPage, roles.PageSize, roles.TotalCount, roles.TotalPages);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET A ROLE IN THE SYSTEM WITH ROLEID = ID
        /// </summary>
        [RequiredFunctionalityName("GetRole")]
        [HttpGet("Roles/{id}")]
        public async Task<ActionResult> GetRoles([FromRoute] int id)
        {
            var result = await _roleManagementRepository.GetRoles(id);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<RoleResponse>((Role)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// UPDATE ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PutRoles")]
        [HttpPost("Roles/Update")]
        public async Task<ActionResult> PutRoles([FromBody] List<RoleResponse> roleResponses)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.UpdateRoles(roleResponses);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
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
        /// DELETE ROLES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("DeleteRoles")]
        [HttpPost("Roles/Delete")]
        public async Task<ActionResult> DeleteRoles([FromBody] List<int> rolesIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.DeleteRoles(rolesIds);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<RoleResponse>>(((List<Role>)result.ObjectValue));
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
        /// ASSIGN ROLES TO USERS IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostAssignRolesToUser")]
        [HttpPost("Users/Roles/Assign")]
        public async Task<ActionResult> PostAssignRolesToUser([FromBody] RoleUserAssignmentRequest roleAssignmentRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.AssignRolesToUser(roleAssignmentRequest);

            if (result.StatusCode == Utils.Success)
            {
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
        /// CREATE PROJECT MODULES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostProjectModules")]
        [HttpPost("ProjectModules/Create")]
        public async Task<ActionResult> PostProjectModule([FromBody] List<ProjectModuleRequest> projectModules)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.CreateProjectModule(_mapper.Map<List<ProjectModule>>(projectModules));

            if (result.StatusCode == Utils.Success)
            {
                var listOfProjectModules = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
                result.ObjectValue = listOfProjectModules;
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
        /// GET ALL PROJECT MODULES TOGETHER WITH THEIR FUNCTIONAITIES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetProjectModules")]
        [HttpGet("ProjectModules")]
        public async Task<ActionResult> GetProjectModules([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetProjectModules(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var projectModules = (PagedList<ProjectModule>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(projectModules.ToList());
                Response.AddPagination(projectModules.CurrentPage, projectModules.PageSize, projectModules.TotalCount, projectModules.TotalPages);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET ALL PROJECT MODULES TOGETHER WITH THEIR FUNCTIONAITIES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("GetProjectModule")]
        [HttpGet("ProjectModules/{projectModuleId}")]
        public async Task<ActionResult> GetProjectModule([FromRoute] int projectModuleId)
        {
            var result = await _roleManagementRepository.GetProjectModules(projectModuleId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<ProjectModuleResponse>((ProjectModule)result.ObjectValue);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE PROJECT MODULES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("DeleteProjectModule")]
        [HttpPost("ProjectModules/Delete")]
        public async Task<ActionResult> DeleteProjectModule([FromBody] List<int> projectModulesIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.DeleteProjectModule(projectModulesIds);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<ProjectModuleResponse>>(((List<ProjectModule>)result.ObjectValue));
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
        /// CREATE FUNCTIONALITIES IN THE SYSTEM
        /// </summary>
        //[RequiredFunctionalityName("PostFunctionality")]
        [RequiredFunctionalityName("PostFunctionalities")]
        [HttpPost("Functionalities/Create")]
        public async Task<ActionResult> PostFunctionality([FromBody] List<FunctionalityRequest> functionalities)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.CreateFunctionality(_mapper.Map<List<Functionality>>(functionalities));

            if (result.StatusCode == Utils.Success)
            {
                var listOfFunctionalities = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
                result.ObjectValue = listOfFunctionalities;
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
        /// GET FUNCTIONALITIES IN THE SYSTEM (USE GET PROJECT MODULES ROUTE)
        /// </summary>
        [RequiredFunctionalityName("GetFunctionalities")]
        [HttpGet("Functionalities")]
        public async Task<ActionResult> GetFunctionalities([FromQuery] UserParams userParams)
        {
            var result = await _roleManagementRepository.GetFunctionalities(userParams);

            if (result.StatusCode == Utils.Success)
            {
                var functionalities = (PagedList<Functionality>)result.ObjectValue;
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(functionalities.ToList());
                Response.AddPagination(functionalities.CurrentPage, functionalities.PageSize, functionalities.TotalCount, functionalities.TotalPages);

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET FUNCTIONALITIES IN THE SYSTEM (USE GET PROJECT MODULES ROUTE)
        /// </summary>
        [RequiredFunctionalityName("GetFunctionality")]
        [HttpGet("Functionalities/{functionalityId}")]
        public async Task<ActionResult> GetFunctionalities([FromRoute] int functionalityId)
        {
            var result = await _roleManagementRepository.GetFunctionalities(functionalityId);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<FunctionalityResponse>((Functionality)result.ObjectValue);
                
                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// DELETE FUNCTIONALITIES IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("DeleteFunctionality")]
        [HttpPost("Functionalities/Delete")]
        public async Task<ActionResult> DeleteFunctionality([FromBody] List<int> functionalitiesIds)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.DeleteFunctionality(functionalitiesIds);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityResponse>>(((List<Functionality>)result.ObjectValue));
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
        /// ASSIGN FUNCTIONALITIES TO ROLE IN THE SYSTEM
        /// </summary>
        [RequiredFunctionalityName("PostAssignFunctionalitiesToRole")]
        [HttpPost("Role/Functionalities/Assign")]
        public async Task<ActionResult> PostAssignFunctionalitiesToRole([FromBody] FunctionalityRoleAssignmentRequest functionalityRoleAssignmentRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _roleManagementRepository.AssignFunctionalitiesToRole(functionalityRoleAssignmentRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.ObjectValue = _mapper.Map<List<FunctionalityRoleResponse>>(((List<FunctionalityRole>)result.ObjectValue));
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