
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Converters;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;

namespace MumArchitecture.Domain.Dtos
{
    public class RoleUserListDto:ListDto
    {
        public int UserId { get; set; }
        public int RoleId { get; set; }
        public UserListDto? User { get; set; }
        public RoleListDto? Role { get; set; }

        public static implicit operator RoleUserListDto?(RoleUser? entity)
        {
            if (entity == null) return null;
            return new RoleUserListDto
            {
                UserId = entity.UserId.ToPublicId(),
                RoleId = entity.RoleId.ToPublicId(),
                User = entity.User,
                Role = entity.Role,
                Id=entity.Id.ToPublicId()
            };
        }
    }
}

