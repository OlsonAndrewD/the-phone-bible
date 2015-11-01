using Microsoft.AspNet.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using Twilio.TwiML;

namespace Garnet.Api.ActionResults
{
    public class TwilioResponseResult : ContentResult
    {
        public TwilioResponseResult(Action<TwilioResponse> buildResponse)
        {
            var response = new TwilioResponse();
            buildResponse(response);
            Content = response.Element.ToString();
            ContentType = new MediaTypeHeaderValue("text/xml");
        }
    }
}
