using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class MailBox : Entity
    {
        public string? To { get; set; }
        public string? From { get; set; }
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public ESendStatus Status { get; set; }
        public int TryCount { get; set; }
        public DateTime SendTime { get; set; }
        public int OpeningCount { get; set; }


    }
}
