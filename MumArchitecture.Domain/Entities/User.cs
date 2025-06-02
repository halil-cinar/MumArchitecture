using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Entities
{
    public class User : Entity
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public bool EmailVerified { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? District { get; set; }
        public string? IdentityNumber { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public bool IsNotificationAllowed { get; set; }
        public string? Key { get; set; }
    }
}
