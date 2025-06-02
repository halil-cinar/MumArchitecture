using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    public class SanitizeAttribute : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
            {
                return ValidationResult.Success;
            }
            var sanitizedInput = SanitizeInput(value?.ToString() ?? "");

            var property = validationContext.ObjectType.GetProperty(validationContext.MemberName ?? "");
            if (property != null && property.CanWrite)
            {
                property.SetValue(validationContext.ObjectInstance, sanitizedInput);
            }

            return ValidationResult.Success;
        }


        private string SanitizeInput(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return input;
            }
            return Regex.Replace(input, @"('|;|--|/\*|\*/)", "");

        }

    }
}
