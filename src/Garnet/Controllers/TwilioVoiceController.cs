using Garnet.Services.Interfaces;
using Microsoft.AspNet.Mvc;
using System;
using Twilio.TwiML;

namespace Garnet.Api.Controllers
{
    [Route(Routes.Root)]
    public class TwilioVoiceController : Controller
    {
        private readonly IContentService _contentService;

        public TwilioVoiceController(IContentService contentService)
        {
            _contentService = contentService;
        }

        private static class Routes
        {
            public const string Root = "api/twilio/voice";
            public const string CurrentContent = "current-content";
            public const string MainMenu = "main-menu";
        }

        [Route(Routes.CurrentContent)]
        [HttpGet]
        public IActionResult GetCurrentContent()
        {
            return TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.Say("Hello, here is your audio. For main menu, press pound anytime.");
                x.Play(_contentService.GetCurrentContentUrl());
                x.EndGather();
                x.Redirect(Routes.MainMenu, "get");
            });
        }

        [Route(Routes.MainMenu)]
        [HttpGet]
        public IActionResult GetMainMenu()
        {
            return TwilioResponseResult(x =>
            {
                x.Say("Main menu is not yet implemented. Goodbye.");
            });
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
