using MumArchitecture.Domain.Converters;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Dtos
{
    public class MailBoxDto
    {

        [LocalizedRequired]
        public int Id { get; set; }
        
        [Email, Sanitize]
        public string? To { get; set; }

        [Email, Sanitize]
        public string? From { get; set; }

        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string? Subject { get; set; }

        [LocalizedRequired, LocalizedMaxLength(4000), Sanitize]
        public string? Content { get; set; }

        [LocalizedRequired]
        public ESendStatus Status { get; set; }
        public bool SendNow { get; set; }

        public static implicit operator MailBoxDto(MailBox entity)
        {
            if (entity == null)
                return null!;

            return new MailBoxDto
            {
                Id = entity.Id.ToPublicId(),
                To = entity.To,
                From = entity.From,
                Subject = entity.Subject,
                Content = entity.Content,
                Status = entity.Status,
            };
        }

        public static explicit operator MailBox(MailBoxDto dto)
        {
            if (dto == null)
                return null!;

            return new MailBox
            {
                Id = dto.Id.ToDatabaseId(),
                To = dto.To,
                From = dto.From,
                Subject = dto.Subject,
                Content = dto.Content,
                Status = dto.Status,
            };
        }
    }

}
