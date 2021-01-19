using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Interfaces;
using MercuryMartAPI.Models;
using System.Net;
using Microsoft.AspNetCore.Identity;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System;
using Microsoft.EntityFrameworkCore;
using System.Text;
using MercuryMartAPI.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using MercuryMartAPI.Dtos;
using MercuryMartAPI.DTOs.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MercuryMartAPI.Dtos.Auth;

namespace MercuryMartAPI.Repositories
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DataContext _dataContext;
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IGlobalRepository _globalRepository;
        private readonly IConfiguration _configuration;
        private readonly IMailRepository _mailRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthRepository(DataContext dataContext, UserManager<User> userManager, IConfiguration configuration, IMailRepository mailRepository, SignInManager<User> signInManager, IGlobalRepository globalRepository, IHttpContextAccessor httpContextAccessor)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _signInManager = signInManager;
            _globalRepository = globalRepository;
            _configuration = configuration;
            _mailRepository = mailRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<ReturnResponse> LoginUser(UserForLoginDto userLoginDetails, string secretKey)
        {
            var user = await _userManager.Users.Where(a => a.NormalizedEmail == userLoginDetails.EmailAddress.ToUpper()).Include(b => b.UserRoles).ThenInclude(c => c.Role).FirstOrDefaultAsync();

            if (user == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.SignInError,
                    StatusMessage = Utils.StatusMessageSignInError
                };
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, userLoginDetails.Password, false);
            if (result.Succeeded)
            {
                object userInfo = null;
                if (user.UserType == Utils.Customer)
                {
                    userInfo = _dataContext.Customer.Where(a => a.CustomerId == user.UserTypeId).Include(b => b.CustomerCartItems).FirstOrDefaultAsync();
                }
                else if (user.UserType == Utils.Administrator)
                {
                    userInfo = await _dataContext.Administrator.Where(a => a.AdministratorId == user.UserTypeId).FirstOrDefaultAsync();
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.InvalidUserType,
                        StatusMessage = Utils.StatusMessageInvalidUserType
                    };
                }

                user.SecondToLastLoginDateTime = user.LastLoginDateTime;
                user.LastLoginDateTime = DateTimeOffset.Now;
                var updateResult = await _userManager.UpdateAsync(user);
                if(!updateResult.Succeeded)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }

                return new ReturnResponse()
                {
                    StatusCode = Utils.Success,
                    ObjectValue = new UserDetails
                    {
                        Token = GenerateJwtToken(user, secretKey),
                        User = user,
                        userProfile = userInfo
                    }
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.SignInError
            };
        }
     
        public async Task<ReturnResponse> VerifyUserEmailAddress(UserEmailRequest userEmailRequest, string secretKey)
        {
            try
            {
                //FIRST OF ALL CONFIRM EMAIL TOKEN BEFORE USING IT TO GET USER DETAILS
                //Continue
                bool emailTokenConfirmed;
                //NO LOGIN REQUIRED TO CONFIRM EMAIL SO...
                //string userEmail = Encoding.UTF8.GetString(Convert.FromBase64String(userEmailRequest.EmailConfirmationLinkToken.Replace("ngiSlauqe", "=")));
                userEmailRequest.EmailConfirmationLinkToken = userEmailRequest.EmailConfirmationLinkToken.Replace('-', '%');
                var originalUserToken = Uri.UnescapeDataString(userEmailRequest.EmailConfirmationLinkToken);
                string[] emailTokenVal = originalUserToken.Split('#', 2);
                string userEmailTokenBase64 = "";
                string userEmailBase64 = "";
                if (emailTokenVal.Length == 1)
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                }
                else
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                    userEmailBase64 = emailTokenVal[1];
                }

                string userEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(userEmailBase64));
                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user != null)
                {
                    if (user.EmailConfirmed)
                    {
                        //EMAIL ALREADY CONFIRMED
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.EmailAlreadyConfirmed,
                            StatusMessage = Utils.StatusMessageEmailAlreadyConfirmed
                        };
                    }
                    else
                    {
                        IdentityResult identityResult = await _userManager.ConfirmEmailAsync(user, userEmailTokenBase64);
                        if (identityResult.Succeeded)
                        {
                            emailTokenConfirmed = true;
                        }
                        else
                        {
                            emailTokenConfirmed = false;
                        }

                        if (emailTokenConfirmed)
                        {
                            //AFTER EMAIL CONFIRMATION AUTOMATICALLY LOG THE USER IN
                            //var appUser = await _userManager.Users.FirstOrDefaultAsync(c => c.NormalizedEmail == user.Email.ToUpper());
                            if((user.UserType == Utils.Customer) || (user.UserType == Utils.Administrator))
                            {
                                //CUSTOMER OR ADMINISTRATOR
                                //AFTER EMAIL CONFIRMATION AUTOMATICALLY LOG THE USER IN
                                var loginResult = await LogUserInWithoutPassword(user);
                                if (loginResult.StatusCode == Utils.Success)
                                {
                                    return loginResult;
                                }
                                else
                                {
                                    return loginResult;
                                }
                            }
                            else
                            {
                                //INVALID USERTYPE
                                return new ReturnResponse()
                                {
                                    StatusCode = Utils.InvalidUserType,
                                    StatusMessage = Utils.StatusMessageInvalidUserType
                                };
                            }
                        }
                        else
                        {
                            return new ReturnResponse()
                            {
                                StatusCode = Utils.NotSucceeded,
                                StatusMessage = Utils.StatusMessageNotSucceeded
                            };
                        }
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }
            catch (NullReferenceException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
        }

        public async Task<ReturnResponse> ResetPasswordSendMail(ResetPasswordRequest resetPasswordRequest)
        {
            if ((resetPasswordRequest == null) || string.IsNullOrWhiteSpace(resetPasswordRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var userDetails = await _userManager.FindByEmailAsync(resetPasswordRequest.EmailAddress);
            if (userDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var userTokenVal = await _userManager.GeneratePasswordResetTokenAsync(userDetails);
            string hashedEmail = GetHashedEmail(userDetails.Email);
            string fullToken = userTokenVal + "#" + hashedEmail;
            var passwordResetLink = GetResetPasswordLink(fullToken);
            if (passwordResetLink == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            var emailSubject = "PASSWORD RESET";
            var currentUserName = "";
            if (userDetails.UserType == Utils.Customer)
            {
                currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == userDetails.UserTypeId).FirstOrDefaultAsync()).FullName;
            }
            else if (userDetails.UserType == Utils.Administrator)
            {
                currentUserName = (await _dataContext.Administrator.Where(c => c.AdministratorId == userDetails.UserTypeId).FirstOrDefaultAsync()).FullName;
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.BadRequest,
                    StatusMessage = Utils.StatusMessageBadRequest
                };
            }
       
            string link = passwordResetLink;
            var emailMessage1 = "";
            var emailMessage2 = "Please click the button below to complete your password reset and activate your account.";

            string emailBody = _globalRepository.GetMailBodyTemplate(currentUserName, "", link, emailMessage1, emailMessage2, "activation.html");


            //SEND MAIL TO CUSTOMER TO RESET PASSWORD
            MailModel mailObj = new MailModel(_configuration.GetValue<string>("MercuryMartEmailAddress"), _configuration.GetValue<string>("MercuryMartEmailName"), resetPasswordRequest.EmailAddress, emailSubject, emailBody);
            var response = await _mailRepository.SendMail(mailObj);
            if(response.StatusCode.Equals(HttpStatusCode.Accepted))
            {
                return new ReturnResponse()
                {
                    StatusMessage = "Mail Sent Successfully",
                    StatusCode = Utils.Success,
                    ObjectValue = userDetails
                };
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.MailFailure,
                    StatusMessage = Utils.StatusMessageMailFailure
                };
            }
        }

        public async Task<ReturnResponse> ResetPasswordSetNewPassword(ResetPasswordRequest resetPasswordRequest)
        {
            //USER SUBMITS THE NEW PASSWORD BEFORE PASSWORD RESET TOKEN IS CONFIRMED
            if ((resetPasswordRequest == null) || string.IsNullOrWhiteSpace(resetPasswordRequest.NewPassword))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };         
            }

            try
            {
                //FIRST OF ALL CONFIRM EMAIL TOKEN BEFORE USING IT TO SET USER PASSWORD
                bool passwordTokenConfirmed;
                //NO LOGIN REQUIRED TO CONFIRM EMAIL
                resetPasswordRequest.PasswordResetLinkToken = resetPasswordRequest.PasswordResetLinkToken.Replace('-', '%');
                var originalUserToken = Uri.UnescapeDataString(resetPasswordRequest.PasswordResetLinkToken);
                string[] emailTokenVal = originalUserToken.Split('#', 2);
                string userEmailTokenBase64 = "";
                string userEmailBase64 = "";
                if (emailTokenVal.Length == 1)
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                }
                else
                {
                    userEmailTokenBase64 = emailTokenVal[0];
                    userEmailBase64 = emailTokenVal[1];
                }

                string userEmail = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(userEmailBase64));
                //string userEmail = Encoding.UTF8.GetString(Convert.FromBase64String(resetPasswordRequest.PasswordResetLinkToken.Replace("ngiSlauqe", "=")));
                var user = await _userManager.FindByEmailAsync(userEmail);

                if (user != null)
                {
                    //HASH THE NEW PASSWORD
                    var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, resetPasswordRequest.NewPassword);

                    //CHECK USER TABLE TO MAKE SURE PASSWORD HAS NOT BEEN USED BEFORE
                    if(user.PasswordHash == newPasswordHash)
                    {
                        //NEW PASSWORD EQUALS CURRENT USER PASSWORD
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NewPasswordError,
                            StatusMessage = Utils.StatusMessageNewPasswordError
                        };
                    }

                    var identityResult = await _userManager.ResetPasswordAsync(user, userEmailTokenBase64, resetPasswordRequest.NewPassword);
                    if (identityResult.Succeeded)
                    {
                        passwordTokenConfirmed = true;
                    }
                    else
                    {
                        passwordTokenConfirmed = false;
                    }

                    if (passwordTokenConfirmed)
                    {
                        //AFTER PASSWORD TOKEN CONFIRMATION...LOG USER IN
                        var loginResult = await LoginUser(new UserForLoginDto()
                        {
                            EmailAddress = userEmail,
                            Password = resetPasswordRequest.NewPassword
                        }, _configuration.GetValue<string>("AppSettings:Secret"));

                        if(loginResult.StatusCode == Utils.Success)
                        {
                            //DO NOTHING
                        }
                        else
                        {
                            //DO NOTHING
                        }

                        return loginResult;
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.NotSucceeded,
                            StatusMessage = Utils.StatusMessageNotSucceeded
                        };                        
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotFound,
                        StatusMessage = Utils.StatusMessageNotFound
                    };
                }
            }
            catch (NullReferenceException)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotSucceeded,
                    StatusMessage = Utils.StatusMessageNotSucceeded
                };
            }
        }

        public string GenerateJwtToken(User user, string secretKey)
        {
            //Get Customer info
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Id.ToString()),
                new Claim(Utils.ClaimType_UserEmail, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.UserTypeId.ToString()),
                new Claim(Utils.ClaimType_UserType, user.UserType.ToString())
            };

            var roles = _userManager.GetRolesAsync(user).Result;

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha512Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.Now.AddDays(1),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        private string GetHashedEmail(string emailVal)
        {
            return Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(emailVal));//.Replace("=","ngiSlauqe");
        }

        private string GetResetPasswordLink(string resetPasswordToken)
        {
            var originUrls = new StringValues();           
            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if(originHeadersGotten)
            {
                var originUrl = originUrls.FirstOrDefault();
                if(string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }

                var convertedToken = Uri.EscapeDataString(resetPasswordToken);
                convertedToken = convertedToken.Replace('%', '-');
                string emailVerificationLink = originUrl + "/" + _configuration.GetValue<string>("UserPasswordResetLink") + "/" + convertedToken;
                return emailVerificationLink;
            }

            return null;
        }

        private string GetResendUserEmailVerificationLink(string userToken)
        {
            var originUrls = new StringValues();
            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if (originHeadersGotten)
            {
                var originUrl = originUrls.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }

                var convertedToken = Uri.EscapeDataString(userToken);
                convertedToken = convertedToken.Replace('%', '-');
                string emailVerificationLink = originUrl + "/" + _configuration.GetValue<string>("UserEmailConfirmationLink") + "/" + convertedToken;
                return emailVerificationLink;
            }

            return null;
        }

        public async Task<ReturnResponse> ResendUserEmailVerificationLink(EmailVerificationRequest emailVerificationRequest)
        {
            if (emailVerificationRequest == null || string.IsNullOrWhiteSpace(emailVerificationRequest.EmailAddress))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull
                };
            }

            var user = await _userManager.FindByEmailAsync(emailVerificationRequest.EmailAddress);
            if(user != null)
            {
                if(user.EmailConfirmed)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.EmailAlreadyConfirmed,
                        StatusMessage = Utils.StatusMessageEmailAlreadyConfirmed
                    };
                }

                //SEND MAIL TO USER TO CONFIRM EMAIL
                var userTokenVal = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                string hashedEmail = GetHashedEmail(user.Email);
                string fullToken = userTokenVal + "#" + hashedEmail;
                var emailVerificationLink = GetResendUserEmailVerificationLink(fullToken);
                if (emailVerificationLink == null)
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.ObjectNull,
                        StatusMessage = Utils.StatusMessageObjectNull
                    };
                }

                var emailSubject = "CONFIRM YOUR EMAIL ADDRESS";

                var currentUserName = "";

                if (user.UserType == Utils.Customer)    
                {
                    currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                }
                else if (user.UserType == Utils.Administrator)
                {
                    currentUserName = (await _dataContext.Administrator.Where(c => c.AdministratorId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                }

                string link = emailVerificationLink;
                var emailMessage1 = "";
                var emailMessage2 = "Please click the button below to complete your email verification and activate you account.";

                string emailBody = _globalRepository.GetMailBodyTemplate(currentUserName, "", link, emailMessage1, emailMessage2, "activation.html");

                //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                MailModel mailObj = new MailModel(_configuration.GetValue<string>("MercuryMartEmailAddress"), _configuration.GetValue<string>("MercuryMartEmailName"), user.Email, emailSubject, emailBody);
                var response = await _mailRepository.SendMail(mailObj);
                if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.Success,
                        StatusMessage = "Email Verification Link Sent Successfully!!!",
                        ObjectValue = user
                    };
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.MailFailure,
                        StatusMessage = Utils.StatusMessageMailFailure
                    };
                }
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.NotFound,
                StatusMessage = Utils.StatusMessageNotFound
            };
        }

        public async Task<ReturnResponse> LogUserInWithoutPassword(User user)
        {
            object userInfo = null;
            if (user.UserType == Utils.Customer)
            {
                //CUSTOMER
                userInfo = await _globalRepository.Get<Customer>(user.UserTypeId);
            }
            else
            {
                //Administrator
                userInfo = await _globalRepository.Get<Administrator>(user.UserTypeId);
            }
            
            if (userInfo == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                ObjectValue = new UserDetails
                {
                    Token = GenerateJwtToken(user, _configuration.GetValue<string>("AppSettings:Secret")),
                    User = user,
                    userProfile = userInfo
                }
            };
        }

        public string GetUserEmailVerificationLink(string userToken)
         {
            var originUrls = new StringValues();
            //CHECK LATER TO SEE IF ANY ORIGIN HEADER WILL BE SENT WITH THE REQUEST IF THE FRONTEND AND BACKEND ARE IN THE SAME DOMAIN...THAT IS IF THERE IS NO CORS
            var originHeadersGotten = _httpContextAccessor.HttpContext.Request.Headers.TryGetValue("Origin", out originUrls);
            if (originHeadersGotten)
            {
                var originUrl = originUrls.FirstOrDefault();
                if (string.IsNullOrWhiteSpace(originUrl))
                {
                    return null;
                }

                var convertedUserToken = Uri.EscapeDataString(userToken);
                convertedUserToken = convertedUserToken.Replace('%', '-');
                string emailVerificationLink = originUrl + "/" + _configuration.GetValue<string>("UserEmailConfirmationLink") + "/" + convertedUserToken;
                return emailVerificationLink;
            }

            return null;
        }

        public async Task<ReturnResponse> ChangePassword(ChangePasswordRequest changePasswordRequest)
        {
            if (changePasswordRequest == null || string.IsNullOrWhiteSpace(changePasswordRequest.OldPassword) || string.IsNullOrWhiteSpace(changePasswordRequest.NewPassword))
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.ObjectNull,
                    StatusMessage = Utils.StatusMessageObjectNull
                };
            }

            if (changePasswordRequest.OldPassword == changePasswordRequest.NewPassword)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.PreviousPasswordStorageError,
                    StatusMessage = Utils.StatusMessagePreviousPasswordStorageError
                };
            }

            var loggedInUser = _globalRepository.GetUserInformation();
            var user = await _userManager.FindByIdAsync(loggedInUser.UserId);

            if (user != null)
            {
                //HASH THE NEW PASSWORD
                var newPasswordHash = _userManager.PasswordHasher.HashPassword(user, changePasswordRequest.NewPassword);
                //CHECK USER TABLE TO MAKE SURE PASSWORD HAS NOT BEEN USED BEFORE
                if (user.PasswordHash.Equals(newPasswordHash))
                {
                    //NEW PASSWORD EQUALS CURRENT USER PASSWORD
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NewPasswordError,
                        StatusMessage = Utils.StatusMessageNewPasswordError
                    };
                }

                var result = await _userManager.ChangePasswordAsync(user, changePasswordRequest.OldPassword, changePasswordRequest.NewPassword);

                if (result.Succeeded)
                {
                    var currentUserName = "";

                    if (user.UserType == Utils.Customer)
                    {
                        currentUserName = (await _dataContext.Customer.Where(c => c.CustomerId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                    }
                    else if (user.UserType == Utils.Administrator)
                    {
                        currentUserName = (await _dataContext.Administrator.Where(c => c.AdministratorId == user.UserTypeId).FirstOrDefaultAsync()).FullName;
                    }

                    string link = "";
                    var emailMessage1 = "";
                    var emailMessage2 = "Your Password has been changed successfully";

                    string emailBody = _globalRepository.GetMailBodyTemplate(currentUserName, "", link, emailMessage1, emailMessage2, "index.html");
                    var emailSubject = "PASSWORD CHANGED SUCCESSFULLY";
                    //SEND MAIL TO CUSTOMER TO VERIFY EMAIL
                    MailModel mailObj = new MailModel(_configuration.GetValue<string>("MercuryMartEmailAddress"), _configuration.GetValue<string>("MercuryMartEmailName"), user.Email, emailSubject, emailBody);
                    var response = await _mailRepository.SendMail(mailObj);
                    if (response.StatusCode.Equals(HttpStatusCode.Accepted))
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.Success,
                            StatusMessage = "Password Changed Successfully",
                            ObjectValue = user
                        };
                    }
                    else
                    {
                        return new ReturnResponse()
                        {
                            StatusCode = Utils.MailFailure,
                            StatusMessage = Utils.StatusMessageMailFailure
                        };
                    }
                }
                else
                {
                    return new ReturnResponse()
                    {
                        StatusCode = Utils.NotSucceeded,
                        StatusMessage = Utils.StatusMessageNotSucceeded
                    };
                }
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

        }

        public async Task<ReturnResponse> GetUser(int userId)
        {
            var user = _userManager.Users.Where(a => a.Id == userId);
            var userDetails = user.FirstOrDefault();
            if (userDetails == null)
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.NotFound,
                    StatusMessage = Utils.StatusMessageNotFound
                };
            }

            var userFullDetails = new User();
            if(userDetails.UserType == Utils.Administrator)
            {
                userFullDetails = await user.Include(c => c.UserRoles).ThenInclude(d => d.Role).Include(b => b.Administrator).FirstOrDefaultAsync();
            }
            else if(userDetails.UserType == Utils.Customer)
            {
                userFullDetails = await user.Include(c => c.UserRoles).ThenInclude(d => d.Role).Include(e => e.Customer).FirstOrDefaultAsync();
            }
            else
            {
                return new ReturnResponse()
                {
                    StatusCode = Utils.InvalidUserType,
                    StatusMessage = Utils.StatusMessageInvalidUserType
                };
            }

            return new ReturnResponse()
            {
                StatusCode = Utils.Success,
                StatusMessage = Utils.StatusMessageSuccess,
                ObjectValue = userFullDetails
            };
        }

        public async Task<bool> UserEmailExists(string userEmailAddress)
        {
            return await _userManager.Users.AnyAsync(x => x.Email.ToUpper() == userEmailAddress.ToUpper());
        }
    }
}
