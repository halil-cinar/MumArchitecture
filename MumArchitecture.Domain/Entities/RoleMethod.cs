using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    [Table("Role_Method")]
    public class RoleMethod:Entity
    {
        public int RoleId { get; set; }
        public int MethodId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public Role? Role { get; set; }

        [ForeignKey(nameof(MethodId))]
        public Method? Method { get; set; }
    }
}
