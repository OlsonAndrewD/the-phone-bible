using Garnet.Api.ActionResults;

namespace Garnet.Api
{
    public interface IBrowser
    {
        TwilioResponseResult HandleBrowse(string phoneNumber, bool navigatingUp);
        TwilioResponseResult HandleSelection(string phoneNumber, string selection);
    }
}
