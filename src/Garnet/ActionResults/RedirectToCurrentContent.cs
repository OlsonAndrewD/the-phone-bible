using Garnet.Api.Extensions;

namespace Garnet.Api.ActionResults
{
    public class RedirectToCurrentContent : TwilioResponseResult
    {
        public RedirectToCurrentContent()
            : base(x =>
            {
                x.AliceSay("One moment please.");
                x.Redirect(Routes.CurrentContent, "get");
            })
        {
        }
    }
}
