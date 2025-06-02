
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;

namespace MumArchitecture.Domain.Dtos
{
    public class RoleListDto:ListDto
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Key { get; set; }



        public static implicit operator RoleListDto(Role? entity)
        {
            if (entity == null) return null!;
            return new RoleListDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                Key = entity.Key
            };
        }
    }
}

