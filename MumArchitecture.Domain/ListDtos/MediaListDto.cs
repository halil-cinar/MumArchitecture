
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
        //dayý yeni iþimiz þöyle burda validatorlar var bunlarýn bir amacý var veri girilen bir deðiþkenin içine doðru ve
        ///ri girildimi diye bakýyor o kadar 
    }
}

