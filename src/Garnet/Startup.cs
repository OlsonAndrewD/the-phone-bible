using Microsoft.AspNet.Builder;
using Microsoft.AspNet.Hosting;
using Microsoft.Framework.Configuration;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.Logging;
using Garnet.DataAccess;
using Garnet.Services;
using Microsoft.AspNet.Mvc;
using Garnet.Api.Filters;

namespace Garnet.Api
{
    public class Startup
    {
        private readonly IConfiguration _configuration;

        public Startup(IHostingEnvironment env)
        {
            var configurationBuilder = new ConfigurationBuilder()
                //.AddJsonFile("config.json")
                .AddJsonFile("secrets.json", optional: true)
                //.AddJsonFile($"config.{env.EnvironmentName}.json", optional: true)
                ;

            //if (env.IsEnvironment("Development"))
            //{
            //    // This reads the configuration keys from the secret store.
            //    // For more details on using the user secret store see http://go.microsoft.com/fwlink/?LinkID=532709
            //    configuration.AddUserSecrets();
            //}

            configurationBuilder.AddEnvironmentVariables();
            _configuration = configurationBuilder.Build();
        }

        // This method gets called by a runtime.
        // Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc(options =>
                {
                    options.Filters.Add(new GoToMainMenuExceptionFilter());
                });

            services.AddTransient<IBrowserFactory, BrowserFactory>();

            services.ConfigureApplicationServices(
                _configuration.Get<string>("DigitalBiblePlatform:ApiKey"), 
                _configuration.Get<string>("GoogleMaps:ApiKey"));

            services.ConfigureApplicationDataAccess(
                _configuration.Get<string>("Redis:ConnectionString"));
        }

        // Configure is called after ConfigureServices is called.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerfactory)
        {
            loggerfactory.AddConsole(minLevel: LogLevel.Warning);

            // Configure the HTTP request pipeline.
            //app.UseStaticFiles();

            // Add MVC to the request pipeline.
            app.UseMvc();
        }
    }
}
