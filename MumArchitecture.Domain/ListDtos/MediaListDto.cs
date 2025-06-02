
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
        //day� yeni i�imiz ��yle burda validatorlar var bunlar�n bir amac� var veri girilen bir de�i�kenin i�ine do�ru ve
        ///ri girildimi diye bak�yor o kadar 
    }
}

