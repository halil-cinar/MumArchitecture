
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;

namespace MumArchitecture.Domain.Dtos
{
    public class MediaListDto:ListDto
    {
        public string? ContentType { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public byte[]? File { get; set; }
    }
}

