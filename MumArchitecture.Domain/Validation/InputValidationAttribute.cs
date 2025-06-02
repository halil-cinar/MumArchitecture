using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    public class InputValidationAttribute : ValidationAttribute
    {
        private static readonly Regex SqlInjectionRegex = new Regex(
        @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|ALTER|EXEC|UNION|CREATE|TRUNCATE)\b|(--|;|'|""|/\*))",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return ValidationResult.Success;

            if (SqlInjectionRegex.IsMatch(value?.ToString() ?? ""))
            {
                return new ValidationResult(Lang.Value("Input contains banned words"));
            }

            return ValidationResult.Success;
        }



    }
}
