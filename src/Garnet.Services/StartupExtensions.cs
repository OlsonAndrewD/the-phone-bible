using Garnet.Domain.Services;
using Garnet.Services.BibleContent;
using Microsoft.Framework.DependencyInjection;

namespace Garnet.Services
{
    public static class StartupExtensions
    {
        public static void ConfigureApplicationServices(this IServiceCollection services, string digitalBiblePlatformApiKey)
        {
            services.AddTransient<IContentService>(serviceProvider =>
                new EsvBibleContentService(digitalBiblePlatformApiKey));
            services.AddTransient<IUserService, UserService>();
        }
    }
}
