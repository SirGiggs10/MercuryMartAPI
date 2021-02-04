using MercuryMartAPI.Data;
using MercuryMartAPI.Dtos;
using MercuryMartAPI.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using CloudinaryDotNet.Actions;
using Microsoft.AspNetCore.Identity;
using MercuryMartAPI.Models;

namespace MercuryMartAPI.Repositories
{
    public class GlobalRepository : IGlobalRepository
    {
        private readonly DataContext _dataContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IWebHostEnvironment _env;
        private readonly ICloudinaryRepository _cloudinaryRepository;
        private readonly UserManager<User> _userManager;


        public GlobalRepository(DataContext dataContext, ICloudinaryRepository cloudinaryRepository, UserManager<User> userManager, IHttpContextAccessor httpContextAccessor, IWebHostEnvironment webHostEnvironment)
        {
            _dataContext = dataContext;
            _userManager = userManager;
            _env = webHostEnvironment;
            _httpContextAccessor = httpContextAccessor;
            _cloudinaryRepository = cloudinaryRepository;
        }

        public bool Add<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dataContext.Add(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool> Add<TEntity>(List<TEntity> entities) where TEntity : class
        {
            try
            {
                await _dataContext.AddRangeAsync(entities.AsEnumerable());
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool Delete<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dataContext.Remove(entity);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public bool Delete<TEntity>(List<TEntity> entities) where TEntity : class
        {
            try
            {
                _dataContext.RemoveRange(entities.AsEnumerable());
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public async Task<bool?> SaveAll()
        {
            try
            {
                int saveStatus = await _dataContext.SaveChangesAsync();
                if (saveStatus > 0)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public bool Update<TEntity>(TEntity entity) where TEntity : class
        {
            try
            {
                _dataContext.Entry(entity).State = EntityState.Modified;
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }
        }

        public bool Update<TEntity>(List<TEntity> entities) where TEntity : class
        {
            try
            {
                _dataContext.UpdateRange(entities.AsEnumerable());
                return true;
            }
            catch(Exception ex)
            {
                return false;
            }       
        }

        public async Task<TEntity> Get<TEntity>(int id) where TEntity : class
        {
            var entity = await _dataContext.FindAsync<TEntity>(id);
            return entity;
        }

        public async Task<List<TEntity>> Get<TEntity>() where TEntity : class
        {
            var entity = await _dataContext.Set<TEntity>().ToListAsync();
            return entity;
        }

        public LoggedInUserInfo GetUserInformation()
        {
            var userId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Name);
            var userTypeId = _httpContextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            var userRoles = _httpContextAccessor.HttpContext.User.Claims.Where(n => n.Type == ClaimTypes.Role).ToList();

            if(userId == null || userTypeId == null)
            {
                return new LoggedInUserInfo()
                {
                    UserId = Utils.UserClaim_Null.ToString(),
                    UserTypeId = Utils.UserClaim_Null
                };
            }

            var userIdVal = userId.Value;
            var userTypeIdVal = Convert.ToInt32(userTypeId.Value);

            if (userRoles == null)
            {
                return new LoggedInUserInfo()
                {
                    UserId = userIdVal,
                    UserTypeId = userTypeIdVal
                };
            }

            return new LoggedInUserInfo()
            {
                UserId = userIdVal,
                UserTypeId = userTypeIdVal,
                Roles = userRoles
            };
        }

        public string GetMailBodyTemplate(string recipientFirstName, string recipientLastName, string link, string message1, string message2, string templateSrc)
        {
            string body = string.Empty;

            //using streamreader for reading html template
            var webRoot = _env.WebRootPath;
            using (StreamReader reader = new StreamReader(Path.Combine(webRoot, templateSrc)))
            {
                body = reader.ReadToEnd();
            }
            
            if (templateSrc == "activation.html")
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
            }
            else
            {
                if (recipientFirstName != "" || recipientLastName != "")
                {
                    body = body.Replace("{firstname}", recipientFirstName);
                    body = body.Replace("{lastname}", recipientLastName);
                }
                body = body.Replace("{link}", link);
                body = body.Replace("{message1}", message1);
                body = body.Replace("{message2}", message2);
            }

            return body;
        }
    }
}
