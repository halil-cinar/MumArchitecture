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
    public class LocalizedPhoneAttribute : ValidationAttribute
    {
        // Türk mobil telefon numaraları için regex:
        // - Başlangıçta opsiyonel olarak "+90" veya "0" olabilir.
        // - Ardından mutlaka "5" ile başlayan ve 9 adet rakam gelen bir yapı beklenir.
        // Örnek geçerli numaralar: "05431234567", "+905431234567"
        private static readonly Regex _phoneRegex = new Regex(
                @"^(?:(?:\+90)|0)?(?:5\d{9}|(?!5)(?:\d{3}\d{7}|\d{4}\d{6}))$",
                RegexOptions.Compiled
            );
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var phone = value as string;

            if (string.IsNullOrWhiteSpace(phone))
                return ValidationResult.Success;

            if (!_phoneRegex.IsMatch(phone))
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return Lang.Value("Please enter a valid phone number.");
        }
    }
}
