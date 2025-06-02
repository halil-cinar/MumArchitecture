using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace MumArchitecture.Business.Extensions
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddScopedServices(this IServiceCollection services, Assembly assembly)
        {
            try
            {
                var types = assembly.GetTypes()?
                    .Where(t => t.IsClass && !t.IsAbstract)
                    .Where(t => t.GetInterfaces()?.Contains(typeof(IAddScope))==true);
                if (types?.Any()==true)
                {
                    foreach (var type in types)
                    {
                        var implementedInterface = type.GetInterfaces().Where(x => x != typeof(IAddScope));
                        if (implementedInterface != null && implementedInterface.Count() > 0)
                        {
                            foreach (var iface in implementedInterface)
                            {
                                services.AddScoped(iface, type);
                            }
                        }
                    }
                }
            }
            catch
            {

            }
            return services;
        }
    }
}
