using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class EmailAttribute : ValidationAttribute
    {
        private static readonly Regex _emailRegex = new Regex(
         @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
         RegexOptions.Compiled | RegexOptions.IgnoreCase
     );

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var email = value as string;

            if (string.IsNullOrWhiteSpace(email))
            {
                return ValidationResult.Success;
            }

            if (!_emailRegex.IsMatch(email))
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return Lang.Value("Please enter a valid email address.");
        }
    }
}
