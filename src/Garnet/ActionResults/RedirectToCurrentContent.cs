namespace Garnet.Api.ActionResults
{
    public class RedirectToCurrentContent : TwilioResponseResult
    {
        public RedirectToCurrentContent()
            : base(x =>
            {
                x.Say("One moment please.");
                x.Redirect(Routes.CurrentContent, "get");
            })
        {
        }
    }
}
