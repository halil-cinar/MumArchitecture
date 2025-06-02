using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.DataAccess
{
    public class DBQuery<T>
    {
        public Expression<Func<T, bool>>[]? Filter { get; set; }
        public Expression<Func<T, object?>>[]? Includes { get; set; }
        public Expression<Func<T, object>>? Order { get; set; }
        public Expression<Func<T, T>>? Select { get; set; }
        public int Skip { get; set; } = 0;
        public int Count { get; set; } = int.MaxValue;
        public bool Descending { get; set; } = false;

    }
}
