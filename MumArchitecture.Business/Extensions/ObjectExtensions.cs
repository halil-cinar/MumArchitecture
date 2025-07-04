using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MumArchitecture.Business.Extensions
{
    
    public static class ObjectExtensions
    {
        public static Dictionary<string, object?> ToDictionary<T>(this T source, bool includeNulls = true)
        {
            if (source is null) throw new ArgumentNullException(nameof(source));

            return source.GetType()
                         .GetProperties(BindingFlags.Public | BindingFlags.Instance)
                         .Where(p => p.CanRead)
                         .Select(p => new { p.Name, Value = p.GetValue(source) })
                         .Where(x => includeNulls || x.Value is not null)
                         .ToDictionary(x => x.Name, x => x.Value);
        }
    }
}
