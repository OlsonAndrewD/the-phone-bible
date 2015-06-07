using Garnet.Domain.Services;
using Microsoft.AspNet.Mvc;
using System;
using Twilio.TwiML;

namespace Garnet.Api.Controllers
{
    [Route(Routes.Root)]
    public class TwilioVoiceController : Controller
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;

        public TwilioVoiceController(IUserService userService, IContentService contentService)
        {
            _userService = userService;
            _contentService = contentService;
        }

        private static class Routes
        {
            public const string Root = "api/twilio/voice";
            public const string Start = "start";
            public const string CurrentContent = "current-content";
            public const string MainMenu = "main-menu";
        }

        [Route(Routes.Start)]
        [HttpGet]
        public IActionResult Start()
        {
            return TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.Say("For main menu, press pound anytime.");
                x.EndGather();
                x.Redirect(Routes.CurrentContent, "get");
            });
        }

        [Route(Routes.Start)]
        [HttpPost]
        public IActionResult HandleStartInput([FromForm(Name = "Digits")] string digits)
        {
            return TwilioResponseResult(x =>
            {
                x.Redirect(digits == "#" ? Routes.MainMenu : Routes.CurrentContent, "get");
            });
        }

        [Route(Routes.CurrentContent)]
        [HttpGet]
        public IActionResult GetCurrentContent([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            var user = _userService.GetOrCreate(fromPhoneNumber);
            return TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.Play(_contentService.GetContentUrl(user.CurrentContentSectionId));
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
                x.BeginGather(new { numDigits = 1 });
                x.Say("Press 1 to hear the current section.");
                x.Say("Press 2 to go to the next section.");
                x.EndGather();
                x.Redirect(Routes.MainMenu, "get");
            });
        }

        [Route(Routes.MainMenu)]
        [HttpPost]
        public IActionResult HandleMainMenuSelection(
            [FromForm(Name = "From")] string fromPhoneNumber, 
            [FromForm(Name = "Digits")] string digits)
        {
            var redirectToContent = digits == "1" || digits == "2";
            var advanceToNextContent = digits == "2";

            if (advanceToNextContent)
            {
                var user = _userService.GetOrCreate(fromPhoneNumber);
                user.CurrentContentSectionId = _contentService.GetSectionAfter(user.CurrentContentSectionId);
                _userService.AddOrUpdate(user);
            }

            if (redirectToContent)
            {
                return TwilioResponseResult(x =>
                {
                    x.Say("One moment please.");
                    x.Redirect(Routes.CurrentContent, "get");
                });
            }

            return TwilioResponseResult(x => x.Redirect(Routes.MainMenu, "get"));
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
