
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class IdentityDto:Dto
    {

        [LocalizedRequired]
        public int UserId { get; set; }

         [LocalizedRequired, LocalizedMinLength(6), Sanitize]
        public string? Password { get; set; }
    }
}

