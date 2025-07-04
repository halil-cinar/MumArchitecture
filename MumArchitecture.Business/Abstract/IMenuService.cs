using MumArchitecture.Business.Result;
using MumArchitecture.Domain.Dtos;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.ListDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Abstract
{
    public interface IMenuService
    {
        public Task<SystemResult<MenuListDto>> Save(MenuDto dto);
        public Task<SystemResult<MenuListDto>> Get(int id);
        public Task<SystemResult<MenuListDto>> Get(Filter<Menu> filter);
        public Task<PaggingResult<MenuListDto>> GetAll(Filter<Menu> filter);
        public Task<SystemResult<List<MenuListDto>>> GetAllTree(Filter<Menu> filter);
        public Task<SystemResult<MenuListDto>> Delete(int id);
        public Task<SystemResult<MenuListDto>> ChangeVisible(int id);
        public Task<SystemResult<MenuListDto>> ChangeActive(int id);
        public Task<SystemResult<List<MenuListDto>>> GetByRoleId(int roleId,EArea area = EArea.Main);
        public Task<SystemResult<List<MenuListDto>>> GetByUserId(int userId,EArea area=EArea.Main);


    }
}
