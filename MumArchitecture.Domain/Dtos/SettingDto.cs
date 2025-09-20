using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Converters;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Dtos
{
    public class SettingDto
    {
        public int Id { get; set; }

        [LocalizedMaxLength(200), Sanitize]
        public string? Name { get; set; }

        [LocalizedMaxLength(200), Sanitize]
        public string? Key { get; set; }

        [LocalizedRequired, LocalizedMaxLength(4000), Sanitize]
        public string? Value { get; set; }

        public List<SettingType> SettingType { get; set; }=new List<SettingType>();


        public static implicit operator SettingDto(Setting entity)
        {
            if (entity == null)
                return null!;

            return new SettingDto
            {
                Id = entity.Id.ToPublicId(),
                Name = entity.Name,
                Key = entity.Key,
                Value = entity.Value,
                SettingType = JsonSerializer.Deserialize<List<SettingType>>(entity.SettingType)??new List<SettingType>(),
            };
        }

        public static explicit operator Setting(SettingDto dto)
        {
            if (dto == null)
                return null!;

            return new Setting
            {
                Id = dto.Id.ToDatabaseId(),
                Name = dto.Name,
                Key = dto.Key,
                Value = dto.Value,
                SettingType = JsonSerializer.Serialize(dto.SettingType),
            };
        }
    }

}
