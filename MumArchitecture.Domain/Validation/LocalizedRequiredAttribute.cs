using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    public class LocalizedRequiredAttribute : RequiredAttribute
    {
        public LocalizedRequiredAttribute() : base()
        {
            ErrorMessage = Lang.Value("{0} must be entered");
        }
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, Lang.Value(name));
        }

    }
}
