
using MumArchitecture.Domain.Abstract;
using MumArchitecture.Domain.Converters;
using MumArchitecture.Domain.Entities;
using MumArchitecture.Domain.Enums;

namespace MumArchitecture.Domain.Dtos
{
    public class MethodListDto:ListDto
    {
        public string? Name { get; set; }

        public static implicit operator MethodListDto(Method? entity)
        {
            if (entity == null) return null!;
            return new MethodListDto
            {
                Id = entity.Id.ToPublicId(),
                Name = entity.Name,
            };
        }

    }
}

