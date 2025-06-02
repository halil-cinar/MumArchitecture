
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;
using MumArchitecture.Domain.Validation;

namespace MumArchitecture.Domain.Dtos
{
    public class MethodDto:Dto
    {

        [LocalizedRequired, LocalizedMaxLength(200), Sanitize]
        public string? Name { get; set; }

        public static implicit operator MethodDto(Method entity)
        {
            if (entity == null)
                return null!;

            return new MethodDto
            {
                Id = entity.Id,
                Name = entity.Name
            };
        }

        public static explicit operator Method(MethodDto dto)
        {
            if (dto == null)
                return null!;

            return new Method
            {
                Id = dto.Id,
                Name = dto.Name
            };
        }
    }
}

