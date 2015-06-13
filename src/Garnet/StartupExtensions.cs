using Microsoft.Framework.DependencyInjection;

namespace Garnet.Api
{
    public static class StartupExtensions
    {
        public static void ConfigureBrowserServices(this IServiceCollection services)
        {
            services.AddTransient<IBrowserFactory, BrowserFactory>();
        }
    }
}
