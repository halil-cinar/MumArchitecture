using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    public class LocalizedCompare : CompareAttribute
    {
        public LocalizedCompare(string otherProperty) : base(otherProperty)
        {
            ErrorMessage = Lang.Value("The values {0} and {1} do not match.") ?? ErrorMessage;
        }
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, Lang.Value(name),Lang.Value(OtherProperty));
        }
    }
}
