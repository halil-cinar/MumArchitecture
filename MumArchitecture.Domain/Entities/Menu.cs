using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class Menu:Entity
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public int DisplayOrder { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsVisible { get; set; } = true;
        public string RoleIds { get; set; } = "[]";//int array
        public virtual Menu? Parent { get; set; }
        public virtual ICollection<Menu> Children { get; set; } = new List<Menu>();
    }
}
