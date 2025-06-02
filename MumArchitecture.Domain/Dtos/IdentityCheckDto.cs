using MumArchitecture.Domain.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Dtos
{
    public class IdentityCheckDto
    {

        [LocalizedRequired, Email, Sanitize]
        public string Email { get; set; }

        [LocalizedRequired, LocalizedMinLength(6), Sanitize]
        public string Password { get; set; }
    }
}
