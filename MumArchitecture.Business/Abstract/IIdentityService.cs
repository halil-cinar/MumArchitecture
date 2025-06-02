using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;

namespace MumArchitecture.Business.Abstract
{
    public interface IIdentityService
    {
        public Task<SystemResult<bool>> Save(IdentityDto ıdentity);
        public Task<SystemResult<bool>> CheckPassword(IdentityCheckDto ıdentity);
        public Task<SystemResult<bool>> ForgatPassword(string email); //1. a�ama
        public Task<SystemResult<bool>> ForgatPassword(string key,IdentityCheckDto ıdentity);//2. a�ama


    }
}