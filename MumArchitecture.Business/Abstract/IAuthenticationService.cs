using Microsoft.VisualStudio.Services.Identity;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Abstract
{
    public interface IAuthenticationService
    {
        public Task<SystemResult<SessionListDto>> Login(IdentityCheckDto identity);
        public Task<SystemResult<Nothing>> LogOut(string token);
        public Task<SystemResult<Nothing>> SignUp(UserDto user);
        public Task<SystemResult<UserListDto>> GetUser(string token);
        public Task<SystemResult<List<MethodListDto>>> GetUserMethods(string token);
        internal Task<int?> GetUserIdFromJWTToken(string token);
        internal Task<SessionListDto?> GetSession(string token);

        public Task<SystemResult<SessionListDto>> CreateSession();

        public int? AuthUserId { get; }
        public string AuthToken { get; }
        public bool IsLogin { get;}

    }
}
