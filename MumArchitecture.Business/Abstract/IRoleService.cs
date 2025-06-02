using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;

namespace MumArchitecture.Business.Abstract
{
    public interface IRoleService
    {
        public Task<SystemResult<RoleListDto>> Save(RoleDto dto);
        public Task<SystemResult<RoleListDto>> Get(int id);
        public Task<SystemResult<RoleListDto>> Get(Filter<Role> filter);
        public Task<SystemResult<RoleListDto>> Delete(int id);
        //internal Task<SystemResult<RoleListDto>> Delete(Role entity);
        public Task<SystemResult<List<RoleListDto>>> GetAll(Filter<Role> filter);

        public Task<SystemResult<RoleUserListDto>> SaveRoleUser(RoleUserDto dto);
        public Task<SystemResult<List<RoleUserListDto>>> GetUserRoles(int userId);
        public Task<SystemResult<RoleUserListDto>> DeleteRoleUser(int id);


        public Task<SystemResult<RoleMethodListDto>> SaveRoleMethod(RoleMethodDto dto);
        public Task<SystemResult<List<RoleMethodListDto>>> GetRoleMethods(int roleId);
        public Task<SystemResult<RoleMethodListDto>> DeleteRoleMethod(int id);


        public Task<SystemResult<List<MethodListDto>>> GetUserMethods(int userId);
        public Task<SystemResult<List<MethodListDto>>> GetAllMethods();
    }
}