using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Domain.Abstract
{
    public abstract class Dto
    {
        public int Id { get; set; }

        protected Dto()
        {
            Id = 0;
        }
    }
}
