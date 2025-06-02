using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    [Table("Session")]
    public class Session :Entity
    {
        public int UserId { get; set; }
        public string? Token { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent {  get; set; }
        [ForeignKey(nameof(UserId))]
        public User? User { get; set; }
        
        

    }
}
