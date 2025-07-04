using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.ListDtos
{
    public class ExcelDataListDto
    {
        public string? ContentType { get; set; }
        public string? Name { get; set; }
        public string? Url { get; set; }
        public byte[]? File { get; set; }
        public bool SendingEmail { get; set; }
    }
}
