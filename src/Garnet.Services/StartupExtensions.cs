﻿using Garnet.Domain.Services;
using Microsoft.Framework.DependencyInjection;

namespace Garnet.Services
{
    public static class StartupExtensions
    {
        public static void ConfigureApplicationServices(this IServiceCollection services, string digitalBiblePlatformApiKey)
        {
            services.AddTransient<IBibleMetadataService, BibleMetadataService>();
            services.AddTransient<IContentService>(serviceProvider =>
                new DigitalBiblePlatformContentService(
                    serviceProvider.GetRequiredService<IUserService>(),
                    serviceProvider.GetRequiredService<IBibleMetadataService>(),
                    digitalBiblePlatformApiKey));
            services.AddTransient<IShortUrlService, ShortUrlService>();
            services.AddTransient<IUserService, UserService>();
        }
    }
}
