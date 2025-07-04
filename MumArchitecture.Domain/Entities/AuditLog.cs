using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class AuditLog:Entity
    {
        public DateTime Date { get; set; }
        public string EntityType { get; set; } = string.Empty;
        public int EntityId { get; set; }
        public string EntityJson { get; set; }= string.Empty;
        public int UserId { get; set; }
        public string UserEmail { get; set; } = string.Empty;

    }
}
