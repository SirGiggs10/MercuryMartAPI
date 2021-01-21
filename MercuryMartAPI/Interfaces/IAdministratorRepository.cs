using MercuryMartAPI.Dtos.Administrator;
using MercuryMartAPI.Dtos.General;
using MercuryMartAPI.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MercuryMartAPI.Interfaces
{
    public interface IAdministratorRepository
    {
        public Task<ReturnResponse> CreateAdministrator(AdministratorRequest administratorRequest);
        public Task<ReturnResponse> GetAdministrators(UserParams userParams);
        public Task<ReturnResponse> GetAdministrators(int administratorId);
        public Task<ReturnResponse> UpdateAdministrator(int administratorId, AdministratorToUpdate administrator);
        public Task<ReturnResponse> DeleteAdministrator(List<int> administratorIds);
    }
}
