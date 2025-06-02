
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;

namespace MumArchitecture.Domain.Dtos
{
    public class RoleMethodListDto:ListDto
    {
        public int RoleId { get; set; }
        public int MethodId { get; set; }
        public RoleListDto? Role { get; set; }
        public MethodListDto? Method { get; set; }

        public static implicit operator RoleMethodListDto(RoleMethod? entity)
        {
            if (entity == null) return null!;
            return new RoleMethodListDto
            {
                Id = entity.Id,
                RoleId = entity.RoleId,
                MethodId = entity.MethodId,
                Role = entity.Role,
                Method = entity.Method
            };
        }

    }
}

