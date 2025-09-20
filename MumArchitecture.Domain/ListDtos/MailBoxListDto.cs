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
    public class MailBoxListDto : ListDto
    {
        public string? To { get; set; }
        public string? From { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public ESendStatus Status { get; set; }
        public int TryCount { get; set; }
        public DateTime SendTime { get; set; }
        public int OpeningCount { get; set; }

        public static implicit operator MailBoxListDto(MailBox? entity)
        {
            if (entity == null) return null!;
            return new MailBoxListDto
            {
                Id = entity.Id.ToPublicId(),
                To = entity.To,
                From = entity.From,
                Subject = entity.Subject,
                Content = entity.Content,
                Status = entity.Status,
                TryCount = entity.TryCount,
                SendTime = entity.SendTime,
                OpeningCount = entity.OpeningCount
            };
        }

        public static implicit operator MailBox(MailBoxListDto dto)
        {
            if (dto == null) return null!;
            return new MailBox
            {
                Id = dto.Id.ToDatabaseId(),
                To = dto.To,
                From = dto.From,
                Subject = dto.Subject,
                Content = dto.Content,
                Status = dto.Status,
                TryCount = dto.TryCount,
                SendTime = dto.SendTime,
                OpeningCount = dto.OpeningCount
            };
        }
    }

}
