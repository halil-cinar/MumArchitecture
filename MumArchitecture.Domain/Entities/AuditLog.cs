using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class AuditLog:Entity
    {
        public DateTime Date { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string EntityJson { get; set; }= string.Empty;
        public string OldEntityJson { get; set; }= string.Empty;
        public int? UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;
        public string UpdatedProperties { get; set; } = string.Empty;//key: {oldvalue ,new value}

        [ForeignKey("UserId")]
        public User? User { get; set; }

    }
}
