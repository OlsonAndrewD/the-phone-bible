using Garnet.Api.TwilioRequestHandlers;
using Garnet.Api.TwilioRequestHandlers.Browsers;
using Microsoft.Framework.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Garnet.Api
{
    public class BrowserServiceLocator
    {
        private readonly IServiceProvider _serviceProvider;
        private static Lazy<IDictionary<string, Type>> BrowserTypes = new Lazy<IDictionary<string, Type>>(GetBrowserTypes);

        public BrowserServiceLocator(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public Browser GetBrowser(string name)
        {
            Type browserType;
            if (BrowserTypes.Value.TryGetValue(name ?? TopLevelBrowser.Name, out browserType))
            {
                return _serviceProvider.GetRequiredService(browserType) as Browser;
            }
            else
            {
                return null;
            }
        }

        private static IDictionary<string, Type> GetBrowserTypes()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(x => typeof(Browser).IsAssignableFrom(x))
                .Where(x => !x.IsAbstract)
                .ToDictionary(x =>
                {
                    var nameField = x.GetField("Name", BindingFlags.Public | BindingFlags.Static);
                    return nameField.GetValue(null).ToString();
                });
        }
    }
}
