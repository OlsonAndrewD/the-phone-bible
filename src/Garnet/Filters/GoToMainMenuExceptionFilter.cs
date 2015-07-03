using Garnet.Api.ActionResults;
using Microsoft.AspNet.Mvc;
using Garnet.Api.Extensions;
using Garnet.Api.Routes;

namespace Garnet.Api.Filters
{
    public class GoToMainMenuExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            context.Result = new TwilioResponseResult(x =>
            {
                x.AliceSay("Sorry, an error occurred.");
                x.Redirect(TwilioVoiceRoutes.MainMenu, "get");
            });
        }
    }
}
