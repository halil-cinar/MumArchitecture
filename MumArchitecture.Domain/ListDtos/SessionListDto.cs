using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.ListDtos
{
    public class SessionListDto : ListDto
    {
        public int UserId { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public User? User { get; set; }

        public static implicit operator SessionListDto(Session? entity)
        {
            if (entity == null) return null!;
            return new SessionListDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                Token = entity.Token,
                ExpiresAt = entity.ExpiresAt,
                //IpAddress = entity.IpAddress,
                //UserAgent = entity.UserAgent,
                User = entity.User
            };
        }

        public static implicit operator Session(SessionListDto? dto)
        {
            if (dto == null) return null!;
            return new Session
            {
                Id = dto.Id,
                UserId = dto.UserId,
                Token = dto.Token,
                ExpiresAt = dto.ExpiresAt,
                //IpAddress = dto.IpAddress,
                //UserAgent = dto.UserAgent,
                User = dto.User
            };
        }
    }

}
