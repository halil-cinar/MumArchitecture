using MumArchitecture.Domain.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Result
{
    public class CustomFilter<TEntity,TFilter> :Filter<TEntity> where TEntity : Entity, new()
    {
        public TFilter? Filter { get; set; }
    }
}
