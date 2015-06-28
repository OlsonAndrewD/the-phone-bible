namespace Garnet.Api.ActionResults
{
    public class TwilioRedirectResult : TwilioResponseResult
    {
        public TwilioRedirectResult(string url = null)
            : base(x => x.Redirect(url, "get"))
        {
        }
    }
}
