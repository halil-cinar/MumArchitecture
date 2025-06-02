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
    public class FileSizeAttribute : ValidationAttribute
    {
        private readonly long _maxFileSizeInBytes;

        /// <summary>
        /// MaksiMumArchitecture dosya boyutunu megabayte cinsinden belirtir.
        /// </summary>
        /// <param name="maxFileSizeInBytes">MaksiMumArchitecture izin verilen dosya boyutu (mb)</param>
        public FileSizeAttribute(float maxFileSizeInMegaBytes)
        {
            _maxFileSizeInBytes =(long)(maxFileSizeInMegaBytes*1024*1024);
        }
        
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var file = value as IFormFile;
            if (file == null)
                return ValidationResult.Success;

            if (file.Length > _maxFileSizeInBytes)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return string.Format(Lang.Value("File size {0} It should not be larger than MB."), _maxFileSizeInBytes / (1024*1024));
        }
    }
}
