
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class RoleMethodDto:Dto
    {
        [LocalizedRequired]
        public int RoleId { get; set; }
        [LocalizedRequired]
        public int MethodId { get; set; }

        public static implicit operator RoleMethodDto(RoleMethod entity)
        {
            if (entity == null)
                return null!;

            return new RoleMethodDto
            {
                Id = entity.Id,
                RoleId = entity.RoleId,
                MethodId = entity.MethodId
            };
        }

        public static explicit operator RoleMethod(RoleMethodDto dto)
        {
            if (dto == null)
                return null!;

            return new RoleMethod
            {
                Id = dto.Id,
                RoleId = dto.RoleId,
                MethodId = dto.MethodId
            };
        }
    }
}


