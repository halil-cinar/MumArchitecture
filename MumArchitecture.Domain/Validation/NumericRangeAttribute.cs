using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    public class NumericRangeAttribute : ValidationAttribute
    {
        public double MiniMumArchitecture { get; }
        public double MaxiMumArchitecture { get; }

        public NumericRangeAttribute(double miniMumArchitecture = double.MinValue, double maxiMumArchitecture = double.MaxValue)
        {
            MiniMumArchitecture = miniMumArchitecture;
            MaxiMumArchitecture = maxiMumArchitecture;
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null)
                return ValidationResult.Success;

            try
            {
                double number = Convert.ToDouble(value);

                if (number < MiniMumArchitecture || number > MaxiMumArchitecture)
                {
                    return new ValidationResult(GetErrorMessage());
                }
            }
            catch (Exception)
            {
                return new ValidationResult(GetErrorMessage());
            }

            return ValidationResult.Success;
        }

        public string GetErrorMessage()
        {
            return string.Format(Lang.Value("The value must be between {0} and {1}."), MiniMumArchitecture, MaxiMumArchitecture);
        }
    }
}
