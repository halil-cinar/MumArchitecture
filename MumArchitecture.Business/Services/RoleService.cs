using Microsoft.Extensions.DependencyInjection;
using NLog.Filters;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MumArchitecture.Business.Services
{
    public class RoleService : ServiceBase<Role>, IRoleService, IAddScope
    {
        private readonly IRepository<RoleUser> _roleUserRepository;
        private readonly IRepository<RoleMethod> _roleMethodRepository;
        private readonly IRepository<User> _userRepository;  
        private readonly IRepository<Method> _methodRepository;
        public RoleService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _roleMethodRepository = serviceProvider.GetRequiredService<IRepository<RoleMethod>>();
            _roleUserRepository = serviceProvider.GetRequiredService<IRepository<RoleUser>>();
            _userRepository= serviceProvider.GetRequiredService<IRepository<User>>();
            _methodRepository =serviceProvider.GetRequiredService<IRepository<Method>>();
        }

        public async Task<SystemResult<RoleListDto>> Delete(int id)
        {
            var result = new SystemResult<RoleListDto>();
            using (TransactionScope scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var role = (await Repository.Get(x => x.Id == id)) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                    var exist = await _roleUserRepository.Count(x => x.RoleId == id) > 0;
                    if (exist)
                    {
                        result.AddMessage(Lang.Value("RoleUsersAreFoundCannotDelete"));
                    }
                    var roleMethods = await _roleMethodRepository.GetAll(x => x.RoleId == id) ?? new List<RoleMethod>();
                    foreach (var rolemethod in roleMethods)
                    {
                        await _roleMethodRepository.Delete(x => x.Id == rolemethod.Id);
                    }
                    await Repository.Delete(x => x.Id == id);
                    result.Data = role;
                    scope.Complete();
                }
                catch (UserException ex)
                {
                    result.AddMessage(ex.Message);
                }
                catch (Exception ex)
                {
                    LogManager.LogException(ex);
                    result.AddDefaultErrorMessage(ex);
                }
                scope.Dispose();
            }
            return result;
        }

        public async Task<SystemResult<RoleMethodListDto>> DeleteRoleMethod(int id)
        {
            var result = new SystemResult<RoleMethodListDto>();

            try
            {
                var rolemethod = (await _roleMethodRepository.Delete(x => x.Id == id)) ?? throw new UserException(Lang.Value("RecordNotFound"));
                result.Data = rolemethod;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<RoleUserListDto>> DeleteRoleUser(int id)
        {
            var result = new SystemResult<RoleUserListDto>();

            try
            {
                var roleuser = (await _roleUserRepository.Get(x => x.Id == id)) ?? throw new UserException(Lang.Value("RecordNotFound"));
                var userRolesCount = await _roleUserRepository.Count(x => x.UserId == id);
                if (userRolesCount == 1)
                {
                    throw new UserException(Lang.Value("UserCannotDeleteAllRoles"));
                }
                await _roleUserRepository.Delete(x => x.Id == id);
                result.Data = roleuser;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<RoleListDto>> Get(int id)
        {
            var result = new SystemResult<RoleListDto>();

            try
            {
                var role = (await Repository.Get(new Filter<Role>().AddFilter(x => x.Id == id).AddIncludes(x => x.Methods).ConvertDbQuery())) ?? throw new Exception(Lang.Value("RecordNotFound"));
                result.Data = role;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<RoleListDto>> Get(Filter<Role> filter)
        {
            var result = new SystemResult<RoleListDto>();

            try
            {
                var role = (await Repository.Get(filter.ConvertDbQuery())) ?? throw new Exception(Lang.Value("RecordNotFound"));
                result.Data = role;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<List<RoleListDto>>> GetAll(Filter<Role> filter)
        {
            var result = new SystemResult<List<RoleListDto>>();

            try
            {
                var roles = (await Repository.GetAll(filter.ConvertDbQuery()));
                result.Data = roles.Select(x => (RoleListDto)x).ToList();
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<List<RoleMethodListDto>>> GetRoleMethods(int roleId)
        {
            var result = new SystemResult<List<RoleMethodListDto>>();

            try
            {
                if (roleId < 0)
                {
                    throw new UserException(Lang.Value("ParamaterNotValid"));
                }
                var filter = new Filter<RoleMethod>().AddFilter(x => x.RoleId == roleId).AddIncludes(x => x.Role, x => x.Method).All();
                var rolemethod = (await _roleMethodRepository.GetAll(filter.ConvertDbQuery()));
                result.Data = rolemethod.Select(x => (RoleMethodListDto)x).ToList();
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<List<MethodListDto>>> GetUserMethods(int userId)
        {
            var result = new SystemResult<List<MethodListDto>>();

            try
            {
                if (userId < 0)
                {
                    throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                }

                var roles = (await _roleUserRepository.GetAll(x => x.UserId == userId)).Select(x => x.RoleId) ?? Enumerable.Empty<int>();
                var data = new List<MethodListDto>();
                foreach (var roleId in roles)
                {
                    var filter = Filter<RoleMethod>.CreateFilter(x => x.RoleId == roleId).AddIncludes(x => x.Method!).All();
                    var methods=await _roleMethodRepository.GetAll(filter.ConvertDbQuery());
                    data.AddRange(methods.Select(x=>(MethodListDto)x.Method));
                }

                result.Data = data;
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<List<MethodListDto>>> GetAllMethods()
        {
            var result = new SystemResult<List<MethodListDto>>();

            try
            {
                var methods = await _methodRepository.GetAll();
                result.Data = methods.Select(x => (MethodListDto)x).ToList();
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<List<RoleUserListDto>>> GetUserRoles(int userId)
        {
            var result = new SystemResult<List<RoleUserListDto>>();

            try
            {
                if (userId < 0)
                {
                    throw new UserException(Lang.Value("ParamaterNotValid"));
                }
                var filter= Filter<RoleUser>.CreateFilter(x => x.UserId == userId).AddIncludes(x=>x.User!,x=>x.Role!).All();
                var roles = (await _roleUserRepository.GetAll(filter.ConvertDbQuery()));
                result.Data = roles!.Select(x=>(RoleUserListDto)x!)!.ToList();
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<RoleListDto>> Save(RoleDto dto)
        {
            var result = new SystemResult<RoleListDto>();

            try
            {
                if ((dto.Id <= 0 && dto.methodIds?.Any() != true))
                {
                    throw new UserException(Lang.Value("Methods is Required"));
                }
                
                var exist = await Repository.Count(x=>x.Name == dto.Name)>0;
                if (exist && dto.Id<=0)
                {
                    throw new UserException(Lang.Value(Messages.RecordAlreadyExist));
                }
                var entity = (Role)dto;
                Role? oldEntity=null;
                if (entity.Id > 0)
                {
                    oldEntity=await Repository.Get(x=>x.Id==entity.Id);
                    await Repository.Update(entity);
                }
                else
                {
                    await Repository.Add(entity);
                }
                LogManager.LogDifferences("", entity, oldEntity);
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }
            return result;
        }

        public async Task<SystemResult<RoleMethodListDto>> SaveRoleMethod(RoleMethodDto dto)
        {
            var result = new SystemResult<RoleMethodListDto>();

            try
            {
                // Rol var mı?
                var roleExists = await Repository.Count(x => x.Id == dto.RoleId) > 0;
                if (!roleExists)
                    throw new UserException(Lang.Value(Messages.ParamaterNotValid));

                // Method var mı?
                var methodExists = await _methodRepository.Count(x => x.Id == dto.MethodId) > 0;
                if (!methodExists)
                    throw new UserException(Lang.Value(Messages.ParamaterNotValid));

                // Aynı ilişki zaten var mı? (role-method ilişkisi)
                var alreadyExists = await _roleMethodRepository.Count(x =>
                    x.RoleId == dto.RoleId && x.MethodId == dto.MethodId) > 0;
                if (alreadyExists)
                    throw new UserException(Lang.Value(Messages.RecordAlreadyExist));

                // Yeni ilişki ekle
                var entity = (RoleMethod)dto;
                await _roleMethodRepository.Add(entity);

                // Logla
                LogManager.LogDifferences("", entity, null);
            }
            catch (UserException ex)
            {
                result.AddMessage(ex.Message);
            }
            catch (Exception ex)
            {
                LogManager.LogException(ex);
                result.AddDefaultErrorMessage(ex);
            }

            return result;
        }

        /* public async Task<SystemResult<RoleMethodListDto>> SaveRoleMethod(RoleMethodDto dto)
         {
             var result = new SystemResult<RoleMethodListDto>();

             try
             {
                 var exist = await Repository.Count(x => x.Id == dto.RoleId) > 0;
                 if (!exist)
                 {
                     throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                 }
                 exist = await _methodRepository.Count(x => x.Id == dto.MethodId) > 0;
                 if (!exist)
                 {
                     throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                 }

                 exist = await _methodRepository.Count(x => x.Id == dto.MethodId && x.Id==dto.RoleId) > 0;
                 if (exist)
                 {
                     throw new UserException(Lang.Value(Messages.RecordAlreadyExist));
                 }

                 var entity = (RoleMethod)dto;
                 RoleMethod? oldEntity = null;
                 if (entity.Id > 0)
                 {
                     oldEntity = await _roleMethodRepository.Get(x => x.Id == entity.Id);
                     await _roleMethodRepository.Update(entity);
                 }
                 else
                 {
                     await _roleMethodRepository.Add(entity);
                 }
                 LogManager.LogDifferences("", entity, oldEntity);
             }
             catch (UserException ex)
             {
                 result.AddMessage(ex.Message);
             }
             catch (Exception ex)
             {
                 LogManager.LogException(ex);
                 result.AddDefaultErrorMessage(ex);
             }
             return result;
         }
        */

        public async Task<SystemResult<RoleUserListDto>> SaveRoleUser(RoleUserDto dto)
         {
             var result = new SystemResult<RoleUserListDto>();

             try
             {
                 var exist = await Repository.Count(x => x.Id == dto.RoleId) > 0;
                 if (!exist)
                 {
                     throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                 }
                 exist = await _userRepository.Count(x => x.Id == dto.UserId) > 0;
                 if (!exist)
                 {
                     throw new UserException(Lang.Value(Messages.ParamaterNotValid));
                 }
                 exist = await _roleUserRepository.Count(x => x.UserId == dto.UserId && x.RoleId == dto.RoleId) > 0;
                 if (exist)
                 {
                     throw new UserException(Lang.Value(Messages.RecordAlreadyExist));
                 }

                 var entity = (RoleUser)dto;
                 RoleUser? oldEntity = null;
                 if (entity.Id > 0)
                 {
                     oldEntity = await _roleUserRepository.Get(x => x.Id == entity.Id);
                     await _roleUserRepository.Update(entity);
                 }
                 else
                 {
                     await _roleUserRepository.Add(entity);
                 }
                 LogManager.LogDifferences("", entity, oldEntity);
             }
             catch (UserException ex)
             {
                 result.AddMessage(ex.Message);
             }
             catch (Exception ex)
             {
                 LogManager.LogException(ex);
                 result.AddDefaultErrorMessage(ex);
             }
             return result;
         }
     }
 }
        
 

