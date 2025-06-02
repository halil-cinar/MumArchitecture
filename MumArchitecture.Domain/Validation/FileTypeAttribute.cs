using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class FileTypeAttribute : ValidationAttribute
    {
        private readonly string[] _allowedExtensions;
        /// <summary>
        /// [FileType(".jpg", ".png")]
        /// </summary>
        /// <param name="allowedExtensions"></param>
        public FileTypeAttribute(params string[] allowedExtensions)
        {
            _allowedExtensions = allowedExtensions;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = (IFormFile?)value;
            if (file == null)
                return ValidationResult.Success;

            var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant();

            if (!_allowedExtensions.Any(ext => ext.ToLowerInvariant() == extension))
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return Lang.Value($"Invalid file type. Valid file types:" +string.Join(", ", _allowedExtensions));
        }
    }
}
