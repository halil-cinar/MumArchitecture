using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Dtos;
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
    public class MenuListDto:ListDto
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? ParentName { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public List<int> RoleIds { get; set; } = new List<int>();
        public virtual MenuListDto? Parent { get; set; }
        public virtual ICollection<MenuListDto> Children { get; set; } = new List<MenuListDto>();
        public EArea Area { get; set; }

        public static implicit operator MenuListDto(Menu? entity)
        {
            if (entity == null) return null!;
            return new MenuListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                DisplayOrder = entity.DisplayOrder,
                Description = entity.Description,
                Url = entity.Url,
                Icon = entity.Icon,
                ParentId = entity.ParentId,
                IsActive = entity.IsActive,
                IsVisible = entity.IsVisible,
                RoleIds = JsonSerializer.Deserialize<List<int>>(entity.RoleIds)??new List<int>(),
                //Parent = entity.Parent,
                ParentName=entity.Parent?.Name,
                //Children = entity.Children.Select(x=>(MenuListDto)x).ToList(),
                Area=entity.Area,   
            };
        }
    }
}
