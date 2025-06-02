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
    public interface ISettingService
    {
        public Task<SystemResult<SettingListDto>> Save(SettingDto dto);
        public Task<SystemResult<SettingListDto>> Get(int id);
        public Task<SystemResult<SettingListDto>> Get(Filter<Setting> filter);
        public Task<PaggingResult<SettingListDto>> GetAll(Filter<Setting> filter);

    }
}
