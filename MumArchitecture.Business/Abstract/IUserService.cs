using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;

namespace MumArchitecture.Business.Abstract
{
    public interface IUserService
    {
        public Task<SystemResult<UserListDto>> Save(UserDto dto);
        public Task<SystemResult<UserListDto>> ChangeActive(int userId);
        public Task<SystemResult<UserListDto>> ChangeNotificationAllowed(int userId);
        public Task<SystemResult<Nothing>> EmailVerifie(string key);
        public Task<SystemResult<UserListDto>> Get(int id);
        public Task<SystemResult<UserListDto>> Get(Filter<User> filter);
        public Task<PaggingResult<UserListDto>> GetAll(Filter<User> filter);       
    }
}
