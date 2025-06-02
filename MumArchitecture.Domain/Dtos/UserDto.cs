using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class UserDto : Dto
    {
        [LocalizedRequired, LocalizedMaxLength(100), Sanitize]
        public string? Name { get; set; }

        [LocalizedRequired, LocalizedMaxLength(100), Sanitize]
        public string? Surname { get; set; }

        [LocalizedPhone, Sanitize]
        public string? Phone { get; set; }

        [Email, Sanitize]
        public string? Email { get; set; }
        //public bool EmailVerified { get; set; }

        [LocalizedMaxLength(4000), Sanitize]
        public string? Address { get; set; }

        [LocalizedMaxLength(100), Sanitize]
        public string? City { get; set; }

        [LocalizedMaxLength(100), Sanitize]
        public string? District { get; set; }

        [ Sanitize]
        public string? IdentityNumber { get; set; }

        [LocalizedMaxLength(1000), Sanitize]
        public string? Description { get; set; }
        //public bool IsActive { get; set; }
        public bool IsNotificationAllowed { get; set; }

        [Sanitize]
        public string? Password { get; set; }

        public bool IsEdit { get; set; }

        public static implicit operator UserDto(User entity)
        {
            if (entity == null)
                return null!;

            return new UserDto
            {
                Id = entity.Id,
                Name = entity.Name,
                Surname = entity.Surname,
                Phone = entity.Phone,
                Email = entity.Email,
                //EmailVerified = entity.EmailVerified,
                Address = entity.Address,
                City = entity.City,
                District = entity.District,
                IdentityNumber = entity.IdentityNumber,
                Description = entity.Description,
                //IsActive = entity.IsActive,
                IsNotificationAllowed = entity.IsNotificationAllowed,
                
                
            };
        }

        public static explicit operator User(UserDto dto)
        {
            if (dto == null)
                return null!;

            return new User
            {
                Id = dto.Id,
                Name = dto.Name,
                Surname = dto.Surname,
                Phone = dto.Phone,
                Email = dto.Email,
                //EmailVerified = dto.EmailVerified,
                Address = dto.Address,
                City = dto.City,
                District = dto.District,
                IdentityNumber = dto.IdentityNumber,
                Description = dto.Description,
                //IsActive = dto.IsActive,
                IsNotificationAllowed = dto.IsNotificationAllowed,
                
            };
        }
    }
}