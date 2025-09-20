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
    public class NotificationContentDto
    {
        public int Id { get; set; }

        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string? Subject { get; set; }

        [LocalizedRequired, LocalizedMaxLength(4000), Sanitize]
        public string? Content { get; set; }
        public List<string>? Variables { get; set; }

        [LocalizedRequired]
        public EContentTarget ContentTarget { get; set; }

        [LocalizedRequired]
        public EContentType ContentType { get; set; }

        [LocalizedRequired]
        public EContentLang ContentLang { get; set; }

        public static implicit operator NotificationContentDto(NotificationContent entity)
        {
            if (entity == null)
                return null!;

            return new NotificationContentDto
            {
                Id = entity.Id.ToPublicId() ,
                Subject = entity.Subject,
                Content = entity.Content,
                Variables = entity.Variables?.Split(",")?.ToList(),
                ContentTarget = entity.ContentTarget,
                ContentType = entity.ContentType,
                ContentLang = entity.ContentLang
            };
        }

        public static explicit operator NotificationContent(NotificationContentDto dto)
        {
            if (dto == null)
                return null!;

            return new NotificationContent
            {
                Id = dto.Id.ToDatabaseId(),
                Subject = dto.Subject,
                Content = dto.Content,
                Variables = string.Join(",",dto.Variables??new List<string>()),
                ContentTarget = dto.ContentTarget,
                ContentType = dto.ContentType,
                ContentLang = dto.ContentLang
            };
        }
    }

}
