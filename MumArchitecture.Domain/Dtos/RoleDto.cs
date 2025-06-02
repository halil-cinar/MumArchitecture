
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class RoleDto:Dto
    {

        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string? Name { get; set; }

        [LocalizedMaxLength(2000), Sanitize]
        public string? Description { get; set; }
        //public string? Key { get; set; }

        [LocalizedRequired]
        public int[]? methodIds { get; set; }

        public static implicit operator RoleDto(Role entity)
        {
            if (entity == null)
                return null!;

            return new RoleDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Description = entity.Description,
                //Key = entity.Key
            };
        }

        public static explicit operator Role(RoleDto dto)
        {
            if (dto == null)
                return null!;

            return new Role
            {
                Id = dto.Id,
                Name = dto.Name,
                Description = dto.Description,
                //Key = dto.Key
            };
        }
    }
}

