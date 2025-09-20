using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Converters;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;

namespace MumArchitecture.Domain.Dtos
{
    public class UserListDto : ListDto
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsNotificationAllowed { get; set; }
        public string? Password { get; set; }
        public bool IsEdit { get; set; }

        public static implicit operator UserListDto(User? entity)
        {
            if (entity == null) return null!;
            return new UserListDto
            {
                Id = entity.Id.ToPublicId(),
                Name = entity.Name,
                Surname = entity.Surname,
                Phone = entity.Phone,
                Email = entity.Email,
                EmailVerified = entity.EmailVerified,
                Address = entity.Address,
                City = entity.City,
                District = entity.District,
                IdentityNumber = entity.IdentityNumber,
                Description = entity.Description,
                IsActive = entity.IsActive,
                IsNotificationAllowed = entity.IsNotificationAllowed,
            };
        }

    }
}