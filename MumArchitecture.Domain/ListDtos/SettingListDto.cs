using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.ListDtos
{
    public class SettingListDto : ListDto
    {
        public string? Name { get; set; }
        public string? Key { get; set; }
        public string? Value { get; set; }
        public string? Type
        {
            get
            {
                switch (SettingType)
                {
                    case ESetting.TEXT:
                        return "string";
                    case ESetting.FILE:
                        return "file";
                    case ESetting.HTML:
                        return "html";
                    case ESetting.BOOL:
                        return "bool";
                    case ESetting.FAQ:
                        return "faq";
                    default:
                        return "string";
                }
            }
        }  //todo: setting type a göre düzenleecek
        public ESetting SettingType { get; set; }
        public string? Category { get; set; }

        public static implicit operator SettingListDto(Setting? entity)
        {
            if (entity == null) return null!;
            return new SettingListDto
            {

                Id = entity.Id,
                Name = entity.Name,
                Key = entity.Key,
                Value = entity.Value,
                SettingType = entity.SettingType,
                Category=entity.Category
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
                SettingType = dto.SettingType,
                Category= dto.Category
            };
        }
    }

}
