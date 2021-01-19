using Microsoft.AspNetCore.Identity;
using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MercuryMartAPI.Dtos;
using MercuryMartAPI.DTOs.Auth;
using MercuryMartAPI.Dtos.Auth;

namespace MercuryMartAPI.Interfaces
{
    public interface IAuthRepository
    {
        public Task<ReturnResponse> LoginUser(UserForLoginDto userLoginDetails, string secretKey);    
        public Task<ReturnResponse> VerifyUserEmailAddress(UserEmailRequest userEmailRequest, string secretKey);
        public Task<ReturnResponse> ResetPasswordSendMail(ResetPasswordRequest resetPasswordRequest);
        public Task<ReturnResponse> ChangePassword(ChangePasswordRequest changePasswordRequest);
        public Task<ReturnResponse> ResetPasswordSetNewPassword(ResetPasswordRequest resetPasswordRequest);
        public string GenerateJwtToken(User user, string secretKey);
        public Task<ReturnResponse> ResendUserEmailVerificationLink(EmailVerificationRequest emailVerificationRequest);
        public string GetUserEmailVerificationLink(string userToken);
        public Task<ReturnResponse> GetUser(int userId);
        public Task<bool> UserEmailExists(string userEmailAddress);
    }
}
