using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Dtos
{
    public class SessionDto
    {
        public int Id { get; set; }

        public int?  UserId { get; set; }

        [LocalizedRequired, LocalizedMaxLength(4000), Sanitize]
        public string? Token { get; set; }

        [LocalizedRequired]
        public DateTime ExpiresAt { get; set; }

        [LocalizedMaxLength(100), Sanitize]
        public string? IpAddress { get; set; }

        [LocalizedMaxLength(1000), Sanitize]
        public string? UserAgent { get; set; }

        public static implicit operator SessionDto(Session entity)
        {
            if (entity == null)
                return null!;

            return new SessionDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Token = entity.Token,
                //ExpiresAt = entity.ExpiresAt,
                IpAddress = entity.IpAddress,
                UserAgent = entity.UserAgent
            };
        }

        public static explicit operator Session(SessionDto dto)
        {
            if (dto == null)
                return null!;

            return new Session
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Token = dto.Token,
                ExpiresAt = dto.ExpiresAt,
                IpAddress = dto.IpAddress,
                UserAgent = dto.UserAgent,
            };
        }
    }
}
