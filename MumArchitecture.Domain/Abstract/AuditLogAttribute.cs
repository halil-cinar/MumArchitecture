using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Abstract
{
    [AttributeUsage(AttributeTargets.Class)]
    public class AuditLogAttribute:Attribute
    {
    }
}
