using Garnet.Api.TwilioRequestHandlers;
using Microsoft.Framework.DependencyInjection;
using System.Linq;
using System.Reflection;

namespace Garnet.Api
{
    public static class StartupExtensions
    {
        public static void ConfigureBrowserServices(this IServiceCollection services)
        {
            var browserTypes = Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => typeof(Browser).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract);

            foreach(var type in browserTypes)
            {
                services.AddTransient(type);
            }

            services.AddTransient<BrowserServiceLocator>();
        }
    }
}
