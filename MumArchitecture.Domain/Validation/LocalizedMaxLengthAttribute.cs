using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Validation
{
    public class LocalizedMaxLengthAttribute : MaxLengthAttribute
    {
        public LocalizedMaxLengthAttribute(int length) : base(length)
        {
            ErrorMessage = Lang.Value("The max character limit of {0} is {1}.") ?? ErrorMessage;
        }
        public override string FormatErrorMessage(string name)
        {
            return string.Format(ErrorMessageString, Lang.Value(name),Length);
        }
    }
}
