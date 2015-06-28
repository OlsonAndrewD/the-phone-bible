using Garnet.Domain.Services;
using Garnet.Services.BibleContentServices;
using Microsoft.Framework.DependencyInjection;

namespace Garnet.Services
{
    public static class StartupExtensions
    {
        public static void ConfigureApplicationServices(this IServiceCollection services, string digitalBiblePlatformApiKey)
        {
            services.AddTransient<IBibleMetadataService, BibleMetadataService>();
            services.AddTransient<IContentService>(serviceProvider =>
                new KjvBibleContentService(
                    serviceProvider.GetRequiredService<IUserService>(),
                    serviceProvider.GetRequiredService<IBibleMetadataService>(),
                    digitalBiblePlatformApiKey));
            services.AddTransient<IUserService, UserService>();
        }
    }
}
