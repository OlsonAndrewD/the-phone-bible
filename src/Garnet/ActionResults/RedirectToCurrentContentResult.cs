using Garnet.Api.Extensions;
using Garnet.Api.Routes;

namespace Garnet.Api.ActionResults
{
    public class RedirectToCurrentContentResult : TwilioResponseResult
    {
        public RedirectToCurrentContentResult()
            : base(x =>
            {
                x.AliceSay("One moment please.");
                x.Redirect(TwilioVoiceRoutes.CurrentContent, "get");
            })
        {
        }
    }
}
