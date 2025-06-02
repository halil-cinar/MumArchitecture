
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class RoleUserDto:Dto
    {

        [LocalizedRequired]
        public int UserId { get; set; }

        [LocalizedRequired]
        public int RoleId { get; set; }

        public static implicit operator RoleUserDto(RoleUser entity)
        {
            if (entity == null)
                return null!;

            return new RoleUserDto
            {
                UserId = entity.UserId,
                RoleId = entity.RoleId
            };
        }

        public static explicit operator RoleUser(RoleUserDto dto)
        {
            if (dto == null)
                return null!;

            return new RoleUser
            {
                UserId = dto.UserId,
                RoleId = dto.RoleId
            };
        }
    }
}

