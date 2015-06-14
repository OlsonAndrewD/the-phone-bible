using Twilio.TwiML;

namespace Garnet.Api.Extensions
{
    public static class TwilioResponseExtensions
    {
        public static TwilioResponse AliceSay(this TwilioResponse response, string text)
        {
            return response.Say(text, new { voice = "alice" });
        }
    }
}
