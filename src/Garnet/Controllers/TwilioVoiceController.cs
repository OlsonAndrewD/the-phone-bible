using Garnet.Api.ActionResults;
using Garnet.Api.TwilioRequestHandlers;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using Twilio.TwiML;

namespace Garnet.Api.Controllers
{
    [Route(Routes.Root)]
    public class TwilioVoiceController : Controller
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly MainMenu _mainMenu;
        private readonly BrowserServiceLocator _browserServiceLocator;

        public TwilioVoiceController(IUserService userService, IContentService contentService, MainMenu mainMenu, BrowserServiceLocator browserServiceLocator)
        {
            _userService = userService;
            _contentService = contentService;
            _mainMenu = mainMenu;
            _browserServiceLocator = browserServiceLocator;
        }

        [Route(Routes.Start)]
        [HttpGet]
        public IActionResult Start()
        {
            return new TwilioResponseResult(x =>
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
            return new TwilioRedirectResult(digits == "#" ? Routes.MainMenu : Routes.CurrentContent);
        }

        [Route(Routes.CurrentContent)]
        [HttpGet]
        public IActionResult GetCurrentContent([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            var user = _userService.GetOrCreate(fromPhoneNumber);
            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.Play(_contentService.GetContentUrl(user.CurrentChapter));
                x.EndGather();
                x.Redirect(Routes.MainMenu, "get");
            });
        }

        [Route(Routes.MainMenu)]
        [HttpGet]
        public IActionResult GetMainMenu()
        {
            return _mainMenu.Get();
        }

        [Route(Routes.MainMenu)]
        [HttpPost]
        public IActionResult HandleMainMenuSelection(
            [FromForm(Name = "From")] string phoneNumber, 
            [FromForm(Name = "Digits")] string selection)
        {
            return _mainMenu.HandleSelection(phoneNumber, selection);
        }

        [Route(Routes.Browse)]
        [HttpGet]
        public IActionResult Browse([FromQuery] string sectionOrGroupName)
        {
            return _browserServiceLocator.GetBrowser(sectionOrGroupName).PromptForSelection();
        }

        [Route(Routes.Browse)]
        [HttpPost]
        public IActionResult HandleBrowseSelection(
            [FromForm(Name = "From")] string fromPhoneNumber,
            [FromForm(Name = "Digits")] string selection,
            [FromQuery] string sectionOrGroupName)
        {
            return _browserServiceLocator.GetBrowser(sectionOrGroupName)
                .HandleSelection(fromPhoneNumber, selection);
        }

        internal static string GetBrowseUrl(string sectionOrGroupName)
        {
            return string.Concat(
                Routes.Browse, "?sectionOrGroupName=", WebUtility.UrlEncode(sectionOrGroupName));
        }
    }
}
