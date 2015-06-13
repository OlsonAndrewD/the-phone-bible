using Garnet.Api.ActionResults;

namespace Garnet.Api
{
    public interface IBrowser
    {
        TwilioResponseResult PromptForSelection();
        TwilioResponseResult HandleSelection(string phoneNumber, string selection);
    }
}
