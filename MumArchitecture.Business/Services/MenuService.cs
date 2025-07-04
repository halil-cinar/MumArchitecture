using Microsoft.Extensions.DependencyInjection;
using MumArchitecture.Business.Abstract;
using MumArchitecture.Business.Extensions;
using MumArchitecture.Business.Logging;
using MumArchitecture.Business.Notification;
using MumArchitecture.Business.Result;
using MumArchitecture.Domain;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.ListDtos;
using NLog.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Services
{
    public class MenuService : ServiceBase<Menu>, IMenuService,IAddScope
    {
        private readonly IRoleService _roleService;
        public MenuService(IServiceProvider serviceProvider) : base(serviceProvider)
        {
            _roleService = serviceProvider.GetRequiredService<IRoleService>();
        }

        public async Task<SystemResult<MenuListDto>> ChangeActive(int id)
        {
            var result = new SystemResult<MenuListDto>();
            try
            {
               var entity = await Repository.Get(x=>x.Id==id);
                if (entity == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }
                entity.IsActive = !entity.IsActive;
                await Repository.Update(entity);
                result.Data = (MenuListDto)entity;
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

        public async Task<SystemResult<MenuListDto>> ChangeVisible(int id)
        {
            var result = new SystemResult<MenuListDto>();
            try
            {
                var entity = await Repository.Get(x => x.Id == id);
                if (entity == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }
                entity.IsVisible = !entity.IsVisible;
                await Repository.Update(entity);
                result.Data = (MenuListDto)entity;
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

        public async Task<SystemResult<MenuListDto>> Delete(int id)
        {
            var result = new SystemResult<MenuListDto>();
            try
            {
                var entity = await Repository.Get(Filter<Menu>.CreateFilter(x=>x.Id==id).AddIncludes(x=>x.Parent,x=>x.Children).ConvertDbQuery());
                if (entity == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }
                if(entity.Children.Any())
                {
                    throw new UserException(Lang.Value("MenuHasChildren"));
                }
                await Repository.Delete(x=>x.Id==id);
                result.Data = (MenuListDto)entity;
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

        public async Task<SystemResult<MenuListDto>> Get(int id)
        {
            var result = new SystemResult<MenuListDto>();
            try
            {
                var entity = await Repository.Get(x => x.Id == id);
                if (entity == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }
                result.Data = (MenuListDto)entity;
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

        public async Task<SystemResult<MenuListDto>> Get(Filter<Menu> filter)
        {
            var result = new SystemResult<MenuListDto>();
            try
            {
                var entity = await Repository.Get(filter.ConvertDbQuery());
                if (entity == null)
                {
                    throw new UserException(Lang.Value(Messages.RecordNotFound));
                }
               
                result.Data = (MenuListDto)entity;
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

        public async Task<PaggingResult<MenuListDto>> GetAll(Filter<Menu> filter)
        {
            var result = new PaggingResult<MenuListDto>();
            try
            {
                result.CurrentPage = filter.Page;
                result.ItemCount = filter.Count;
                var count = (await Repository.Count(filter.ConvertDbQuery()));
                result.AllCount = count;
                result.AllPageCount = (int)Math.Ceiling(count / filter.Count * 1.0);
                var mail = await Repository.GetAll(filter.ConvertDbQuery());
                result.Data = mail.Select(x => (MenuListDto)x).ToList();
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

        public async Task<SystemResult<List<MenuListDto>>> GetAllTree(Filter<Menu> filter)
        {
            var result = new SystemResult<List<MenuListDto>>();
            try
            {
                var menus = await Repository.GetAll(filter.ConvertDbQuery());
                var list = menus.Select(x => (MenuListDto)x).ToList();

                var lookup = list.ToLookup(m => m.ParentId);
                List<MenuListDto> Build(int? parentId) =>
                    lookup[parentId]
                        .OrderBy(m => m.DisplayOrder)
                        .Select(m =>
                        {
                            m.Children = Build(m.Id);
                            return m;
                        })
                        .ToList();

                result.Data = Build(null);
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


        public Task<SystemResult<List<MenuListDto>>> GetByRoleId(int roleId,EArea area = EArea.Main)
        {
            return GetAllTree(Filter<Menu>.CreateFilter(x => x.RoleIds.Contains(roleId.ToString()),x=>x.Area==area).AddIncludes(x => x.Parent, x => x.Children));
        }

        public Task<SystemResult<List<MenuListDto>>> GetByUserId(int userId, EArea area = EArea.Main)
        {
            var roleIds = _roleService.GetUserRoles(userId)?.Result?.Data?.Select(x=>x.Id)?.ToList() ?? new List<int>();

            if (!roleIds.Any())
                return Task.FromResult(new SystemResult<List<MenuListDto>>());

            var filter = Filter<Menu>
                .CreateFilter(m => roleIds.Any(rid => m.RoleIds.Contains(rid.ToString())),x=>x.Area==area)
                ;//.AddIncludes( m => m.Children);

            return GetAllTree(filter);
        }

        public async Task<SystemResult<MenuListDto>> Save(MenuDto dto)
        {
            var result = new SystemResult<MenuListDto>();
            try
            {
                var entity= (Menu)dto;
                var oldEntity = await Repository.Get(x => x.Id == dto.Id);
             
                if (oldEntity != null) {
                    await Repository.Update(entity);
                }
                else
                {
                    await Repository.Add(entity);
                }

                result.Data = (MenuListDto)entity;
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
