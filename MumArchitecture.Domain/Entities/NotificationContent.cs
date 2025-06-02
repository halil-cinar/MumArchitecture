using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
 
namespace MumArchitecture.Domain.Entities
{
    [Table("NotificationContent")]
    public class NotificationContent : Entity
    {
        public string? Subject { get; set; }
        public string? Content { get; set; }
        public string? Variables { get; set; }
        public EContentTarget ContentTarget { get; set; }
        public EContentType ContentType { get; set; }
        public EContentLang ContentLang { get; set; }

    }
}
