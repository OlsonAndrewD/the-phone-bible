using Microsoft.AspNet.Mvc;
using System;
using Twilio;
using Twilio.TwiML;

namespace Garnet.Api.Controllers
{
    [Route("api/twilio")]
    public class TwilioController : Controller
    {
        [Route("voice/initial")]
        [HttpGet]
        public IActionResult GetInitialVoiceResponse()
        {
            return TwilioResponseResult(x =>
            {
                x.Play("http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B01___01_Matthew_____ENGESVN2DA.mp3");
            });
        }

        [Route("sms/response")]
        [HttpGet]
        public IActionResult GetSmsResponse()
        {
            return TwilioResponseResult(x => x.Message("Hi there!"));
        }

        private IActionResult TwilioResponseResult(Action<TwilioResponse> buildResponse)
        {
            var response = new TwilioResponse();
            buildResponse(response);
            return new ContentResult
            {
                Content = response.Element.ToString(),
                ContentType = "text/xml"
            };
        }
    }
}
