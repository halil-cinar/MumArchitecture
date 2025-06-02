using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.DataAccess.Abstract;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace MumArchitecture.Business.Services
{
    public class UserService : ServiceBase<User>, IUserService, IAddScope
    {
        private readonly IIdentityService _identityService;
        private readonly IRepository<RoleUser> _roleUserRepository;
        private readonly IRepository<Role> _roleRepository;
        public UserService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _identityService = serviceProvider.GetRequiredService<IIdentityService>();
            _roleUserRepository = serviceProvider.GetRequiredService<IRepository<RoleUser>>();
            _roleRepository = serviceProvider.GetRequiredService<IRepository<Role>>();
        }

        public async Task<SystemResult<UserListDto>> ChangeActive(int userId)
        {
            var result = new SystemResult<UserListDto>();
            try
            {
                var user = await Repository.Get(x => x.Id == userId) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                user.IsActive = !user.IsActive ;
                await Repository.Update(user);
                result.Data = user;
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

        public async Task<SystemResult<UserListDto>> ChangeNotificationAllowed(int userId)
        {
            var result = new SystemResult<UserListDto>();
            try
            {
                var user = await Repository.Get(x => x.Id==userId) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                user.IsNotificationAllowed = true;
                await Repository.Update(user);
                result.Data = user;
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


        public async Task<SystemResult<Nothing>> EmailVerifie(string key)
        {
            var result = new SystemResult<Nothing>();
            try
            {
                var user = await Repository.Get(x => x.Key==key) ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                user.EmailVerified = true;
                await Repository.Update(user);
                result.Data = new Nothing();
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

        public async Task<SystemResult<UserListDto>> Get(int id)
        {
            var result = new SystemResult<UserListDto>();
            try
            {
                var user = await Repository.Get(x => x.Id == id);// ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                result.Data = user;
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

        public async Task<SystemResult<UserListDto>> Get(Filter<User> filter)
        {
            var result = new SystemResult<UserListDto>();
            try
            {
                var user = await Repository.Get(filter.ConvertDbQuery());// ?? throw new UserException(Lang.Value(Messages.RecordNotFound));
                result.Data = user;
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

        public async Task<PaggingResult<UserListDto>> GetAll(Filter<User> filter)
        {
            var result = new PaggingResult<UserListDto>();

            try
            {
                result.CurrentPage = filter.Page;
                result.ItemCount = filter.Count;

                var count = await Repository.Count(filter.ConvertDbQuery());
                result.AllCount = count;

                result.AllPageCount = filter.Count > 0
                    ? (int)Math.Ceiling(count * 1.0 / filter.Count)
                    : 1;

                var users = await Repository.GetAll(filter.ConvertDbQuery());
                result.Data = users.Select(x => (UserListDto)x).ToList();
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


        public async Task<SystemResult<UserListDto>> Save(UserDto dto)
        {
            var result = new SystemResult<UserListDto>();
            using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                try
                {
                    var entity = (User)dto;
                    var oldEntity = await Repository.Get(x => x.Id == entity.Id);
                    if (oldEntity == null)
                    {
                        entity.EmailVerified = false;
                        entity.IsActive = true;
                        entity.Key = Guid.NewGuid().ToString();
                        entity = await Repository.Add(entity);
                        var identityResult = await _identityService.Save(new IdentityDto
                        {
                            UserId = entity.Id,
                            Password = dto.Password,
                        });
                        if (!identityResult.IsSuccess)
                        {
                            result.AddMessage(identityResult);
                            scope.Dispose();
                            return result;
                        }
                        var role = await _roleRepository.Get(x => x.Key == "User")??throw new Exception("Role kayıtları hatalı user rolü eksik");
                        await _roleUserRepository.Add(new RoleUser
                        {
                            RoleId = role.Id,
                            UserId = entity.Id,
                        });
                    }
                    else
                    {
                        entity.EmailVerified = oldEntity.EmailVerified;
                        entity.IsActive = oldEntity.IsActive;
                        await Repository.Update(entity);
                    }
                    result.Data = entity;
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

    }
}
