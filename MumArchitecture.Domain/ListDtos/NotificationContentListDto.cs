using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Converters;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.ListDtos
{
    public class NotificationContentListDto : ListDto
    {
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public List<string>? Variables { get; set; }
        public EContentTarget ContentTarget { get; set; } //Control
        public EContentType ContentType { get; set; }
        public EContentLang ContentLang { get; set; }

        public static implicit operator NotificationContentListDto(NotificationContent? entity)
        {
            if (entity == null) return null!;
            return new NotificationContentListDto
            {
                Id = entity.Id.ToPublicId(),
                Subject = entity.Subject,
                Content = entity.Content,
                Variables = entity.Variables?.Split(",")?.ToList(),
                ContentTarget = entity.ContentTarget,
                ContentType = entity.ContentType,
                ContentLang = entity.ContentLang
            };
        }

    }

}
