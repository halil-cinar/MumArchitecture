using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Dtos
{
    public class MenuDto :Dto
    {
        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string Name { get; set; } = string.Empty;
        [LocalizedMaxLength(2000), Sanitize]
        public string Description { get; set; } = string.Empty;
        [LocalizedRequired, LocalizedMaxLength(500), Sanitize]
        public string Url { get; set; } = string.Empty;
        [Sanitize]
        public string Icon { get; set; } = string.Empty;
        [NumericRange(0)]
        public int? ParentId { get; set; }
        [NumericRange(0)]
        public int DisplayOrder { get; set; } = 0;
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public List<int> MenuIds { get; set; }=new List<int>();


        public static implicit operator MenuDto(Menu entity)
        {
            if (entity == null)
                return null!;

            return new MenuDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                DisplayOrder = entity.DisplayOrder,
                Url = entity.Url,
                Icon = entity.Icon,
                ParentId = entity.ParentId,
                IsActive = entity.IsActive,
                IsVisible = entity.IsVisible,
                MenuIds=JsonSerializer.Deserialize<List<int>>(entity.RoleIds) ?? new List<int>()
            };
        }

        public static explicit operator Menu(MenuDto dto)
        {
            if (dto == null)
                return null!;

            return new Menu
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                DisplayOrder = dto.DisplayOrder,
                Url = dto.Url,
                Icon = dto.Icon,
                ParentId = dto.ParentId,
                IsActive = dto.IsActive,
                IsVisible = dto.IsVisible,
                RoleIds = JsonSerializer.Serialize(dto.MenuIds) ?? "[]"
            };
        }
    }
}
