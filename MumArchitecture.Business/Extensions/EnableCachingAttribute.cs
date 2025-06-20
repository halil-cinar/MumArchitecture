using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Extensions
{
    public sealed class EnableCachingAttribute : Attribute
    {
        public int DurationSeconds { get; }

        public EnableCachingAttribute(int durationSeconds=60)
        {
            DurationSeconds = durationSeconds;
        }
    }

}
