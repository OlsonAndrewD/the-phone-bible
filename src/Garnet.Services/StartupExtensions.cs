using Microsoft.Framework.DependencyInjection;

namespace Garnet.Services
{
    public static class StartupExtensions
    {
        public static void ConfigureApplicationServices(this IServiceCollection services)
        {
            //services.AddTransient<IUsersService, UsersService>();
        }
    }
}
