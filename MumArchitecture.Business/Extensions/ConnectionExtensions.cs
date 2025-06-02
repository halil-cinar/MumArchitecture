using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Extensions
{
    public static class ConnectionExtensions
    {
        public static string GetFullIpAddress(this ConnectionInfo connection)
        {
            var ip = connection.RemoteIpAddress?.ToString()??"";
            var port = connection.RemotePort.ToString();
            return ip + ":" + port;
        }
    }
}
