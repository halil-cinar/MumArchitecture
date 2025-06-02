using MumArchitecture.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business
{
    public class UserClaim
    {
        public int UserID { get; set; }
        public UserDto? User { get; set; }

    }
}
  