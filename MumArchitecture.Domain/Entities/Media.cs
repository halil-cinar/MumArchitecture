using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class Media:Entity
    {
        public string? ContentType { get; set; }
        public string? Name { get; set; }
        public string? Location { get; set; }
        public int SavedUserId { get; set; }
        public string? Url { get; set; }

        [ForeignKey(nameof(SavedUserId))]
        public User? SavedUser { get; set; }
    }
}
