using System;
using System.Text;
using System.Threading.Tasks;
using MercuryMartAPI.Models;
using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MercuryMartAPI.Dtos;
using MercuryMartAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using MercuryMartAPI.Helpers;
using System.Net;
using MercuryMartAPI.Dtos.Customer;
using MercuryMartAPI.Dtos.General;
using Microsoft.AspNetCore.Authorization;
using MercuryMartAPI.DTOs.Auth;
using MercuryMartAPI.Dtos.Auth;
using MercuryMartAPI.Data;
using System.Collections.Generic;
using MercuryMartAPI.Helpers.AuthorizationMiddleware;
using MercuryMartAPI.Dtos.Administrator;
using MercuryMartAPI.Interfaces.Logger;

namespace MercuryMartAPI.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IAuthRepository _authRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly DataContext _dataContext;
        private readonly ILoggerManager _logger;

        public AuthController(UserManager<User> userManager, IAuthRepository authRepository, IMapper mapper, IConfiguration configuration, DataContext dataContext, ILoggerManager logger)
        {
            _userManager = userManager;
            _authRepository = authRepository;
            _mapper = mapper;
            _configuration = configuration;
            _dataContext = dataContext;
            _logger = logger;
        }

        /// <summary>
        /// LOGIN THE USER TO THE SYSTEM
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("Login")]
        public async Task<ActionResult> PostLogin([FromBody] UserForLoginDto userForLoginDto)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            _logger.LogInfo($"UserEmail: {userForLoginDto.EmailAddress} Logging In...");
            var result = await _authRepository.LoginUser(userForLoginDto, _configuration.GetValue<string>("AppSettings:Secret"));

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Login Success!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponseForLogin>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //ADMINISTRATOR
                    userInfoToReturn.UserProfileInformation = _mapper.Map<AdministratorResponse>((Administrator)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
                _logger.LogInfo("Committing...");
                await dbTransaction.CommitAsync();
                _logger.LogInfo($"UserEmail: {userForLoginDto.EmailAddress} Login Successful");

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                _logger.LogInfo("Rolling Back...");
                await dbTransaction.RollbackAsync();
                _logger.LogInfo($"UserEmail: {userForLoginDto.EmailAddress} Login Failed");

                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// VERIFY USER EMAIL ADDRESS
        /// </summary>
        [AllowAnonymous]
        [HttpPost("VerifyUserEmail")]
        public async Task<ActionResult> PostVerifyUserEmailAddress([FromBody] UserEmailRequest userEmailRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.VerifyUserEmailAddress(userEmailRequest, _configuration.GetValue<string>("AppSettings:Secret"));

            if (result.StatusCode == Utils.Success)
            {
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //ADMINISTRATOR
                    userInfoToReturn.UserProfileInformation = _mapper.Map<AdministratorResponse>((Administrator)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
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
        /// CHANGE USER PASSWORD
        /// </summary>
        [RequiredFunctionalityName("PostChangePassword")]
        [HttpPost]
        [Route("ChangePassword")]
        public async Task<ActionResult> PostChangePassword([FromBody] ChangePasswordRequest changePasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ChangePassword(changePasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Password Changed Successfully!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //ADMINISTRATOR
                    userInfoToReturn.UserProfileInformation = _mapper.Map<AdministratorResponse>((Administrator)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
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
        /// SEND PASSWORD RESET LINK TO THE USER'S EMAIL
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword/SendMail")]
        public async Task<ActionResult> PostResetPasswordSendMail([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var result = await _authRepository.ResetPasswordSendMail(resetPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserToReturn>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
           
        }

        /// <summary>
        /// FINALLY RESET USER PASSWORD USING THE PASSWORD RESET LINK SENT EARLIER AND THEN SET NEW PASSWORD FOR THE USER
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResetPassword/SetNewPassword")]
        public async Task<ActionResult> PostResetPasswordSetNewPassword([FromBody] ResetPasswordRequest resetPasswordRequest)
        {
            var dbTransaction = await _dataContext.Database.BeginTransactionAsync();
            var result = await _authRepository.ResetPasswordSetNewPassword(resetPasswordRequest);

            if (result.StatusCode == Utils.Success)
            {
                result.StatusMessage = "Password Set Successfully!!!";
                var userDetails = (UserDetails)result.ObjectValue;
                var userInfoToReturn = _mapper.Map<UserLoginResponse>(userDetails);
                if (userDetails.User.UserType == Utils.Customer)
                {
                    //CUSTOMER
                    userInfoToReturn.UserProfileInformation = _mapper.Map<CustomerResponse>((Customer)userDetails.userProfile);
                }
                else
                {
                    //ADMINISTRATOR
                    userInfoToReturn.UserProfileInformation = _mapper.Map<AdministratorResponse>((Administrator)userDetails.userProfile);
                }

                result.ObjectValue = userInfoToReturn;
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
        /// RESEND EMAIL VERIFICATION LINK TO USERS EMAIL INCASE HE MISSED THE LINK SENT DURING REGISTRATION
        /// </summary>
        [AllowAnonymous]
        [HttpPost]
        [Route("ResendUserEmailVerificationLink")]
        public async Task<ActionResult> PostResendUserEmailVerificationLink([FromBody] EmailVerificationRequest emailVerificationRequest)
        {
            var result = await _authRepository.ResendUserEmailVerificationLink(emailVerificationRequest);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserToReturn>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }

        /// <summary>
        /// GET A USER IN THE SYSTEM USING USERID
        /// </summary>
        [RequiredFunctionalityName("GetUserInSystem")]
        [HttpGet]
        [Route("Users/{userId}")]
        public async Task<ActionResult> GetUser([FromRoute] int userId)
        {
            var result = await _authRepository.GetUser(userId);

            if (result.StatusCode == Utils.Success)
            {
                var userInfoToReturn = _mapper.Map<UserWithUserTypeObjectResponse>((User)result.ObjectValue);
                result.ObjectValue = userInfoToReturn;

                return StatusCode(StatusCodes.Status200OK, result);
            }
            else
            {
                return StatusCode(StatusCodes.Status400BadRequest, result);
            }
        }
    }
}