using Microsoft.AspNetCore.Http;
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class MediaDto : Dto
    {

        [LocalizedRequired]
        public int SavedUserId { get; set; }

        [FileType(".jpg", ".png", ".pdf"), FileSize(10)]
        public IFormFile? File { get; set; }

        [LocalizedMaxLength(2000), Sanitize]
        public string? Message  { get; set; }
    }
}

