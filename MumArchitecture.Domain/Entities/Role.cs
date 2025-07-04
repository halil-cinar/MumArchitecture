using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class Role:Entity
    {
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? Key { get; set; }

        public virtual ICollection<RoleMethod>? Methods { get; set; }
    }
}
