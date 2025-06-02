using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Dtos
{
    public class SettingDto
    {
        public int Id { get; set; }

        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string? Name { get; set; }

        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string? Key { get; set; }

        [LocalizedRequired, LocalizedMaxLength(4000), Sanitize]
        public string? Value { get; set; }

        public ESetting SettingType { get; set; }


        public static implicit operator SettingDto(Setting entity)
        {
            if (entity == null)
                return null!;

            return new SettingDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Key = entity.Key,
                Value = entity.Value,
                SettingType = entity.SettingType,
            };
        }

        public static explicit operator Setting(SettingDto dto)
        {
            if (dto == null)
                return null!;

            return new Setting
            {
                Id = dto.Id,
                Name = dto.Name,
                Key = dto.Key,
                Value = dto.Value,
                SettingType = dto.SettingType,
            };
        }
    }

}
