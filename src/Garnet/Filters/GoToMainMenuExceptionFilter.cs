using Garnet.Api.ActionResults;
using Garnet.Api.Extensions;
using Garnet.Api.Routes;
using Microsoft.AspNet.Mvc.Filters;

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
