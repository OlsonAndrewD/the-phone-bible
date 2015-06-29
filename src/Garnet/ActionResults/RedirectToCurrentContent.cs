using Garnet.Api.Extensions;
using Garnet.Api.Routes;

namespace Garnet.Api.ActionResults
{
    public class RedirectToCurrentContent : TwilioResponseResult
    {
        public RedirectToCurrentContent()
            : base(x =>
            {
                x.AliceSay("One moment please.");
                x.Redirect(TwilioVoiceRoutes.CurrentContent, "get");
            })
        {
        }
    }
}
