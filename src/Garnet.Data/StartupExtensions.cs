using Microsoft.Framework.DependencyInjection;
using StackExchange.Redis;

namespace Garnet.DataAccess
{
    public static class StartupExtensions
    {
        public static void ConfigureApplicationDataAccess(this IServiceCollection services, string redisConnectionString)
        {
            services.AddSingleton(serviceProvider =>
                ConnectionMultiplexer.Connect(redisConnectionString));
        }
    }
}
