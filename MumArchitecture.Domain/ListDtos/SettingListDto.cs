using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.ListDtos
{
    public class SettingListDto : ListDto
    {
        public string? Name { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        
        public List<SettingType>? SettingType { get; set; }

        public static implicit operator SettingListDto(Setting? entity)
        {
            if (entity == null) return null!;
            return new SettingListDto
            {

                Id = entity.Id,
                Name = entity.Name,
                Key = entity.Key,
                Value = entity.Value,
                SettingType = JsonSerializer.Deserialize<List<SettingType>>(entity.SettingType) ?? new List<SettingType>(),
            };
        }

        public static implicit operator Setting(SettingListDto? dto)
        {
            if (dto == null) return null!;
            return new Setting
            {
                Id = dto.Id,
                Name = dto.Name,
                Key = dto.Key,
                Value = dto.Value,
                SettingType = JsonSerializer.Serialize(dto.SettingType),
            };
        }
    }

}
