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
            public const string Browse = "browse";
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
            return TwilioRedirect(digits == "#" ? Routes.MainMenu : Routes.CurrentContent);
        }

        [Route(Routes.CurrentContent)]
        [HttpGet]
        public IActionResult GetCurrentContent([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            var user = _userService.GetOrCreate(fromPhoneNumber);
            return TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.Play(_contentService.GetContentUrl(user.CurrentSectionId));
                x.EndGather();
                x.Redirect(Routes.MainMenu, "get");
            });
        }

        [Route(Routes.MainMenu)]
        [HttpGet]
        public IActionResult GetMainMenu()
        {
            var sectionDescriptor = _contentService.SectionDescriptor;

            return TwilioResponseResult(x =>
            {
                x.BeginGather(new { numDigits = 1 });
                x.Say(string.Concat("Press 1 to hear the current ", sectionDescriptor, "."));
                x.Say(string.Concat("Press 2 to go to the next ", sectionDescriptor, "."));
                x.Say(string.Concat("Press 3 to browse to a different ", sectionDescriptor, "."));
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
                SetUserCurrentSection(user, _contentService.GetSectionAfter(user.CurrentSectionId));
            }

            if (redirectToContent)
            {
                return RedirectToCurrentContent();
            }
            else if (digits == "3")
            {
                return TwilioRedirect(Routes.Browse);
            }

            // TODO: Prevent infinite loop.
            return TwilioRedirect(Routes.MainMenu);
        }

        [Route(Routes.Browse)]
        [HttpGet]
        public IActionResult Browse([FromQuery] string sectionOrGroupName)
        {
            return ListNavOptions(sectionOrGroupName);
        }

        [Route(Routes.Browse)]
        [HttpPost]
        public IActionResult HandleBrowseSelection(
            [FromForm(Name = "From")] string fromPhoneNumber,
            [FromForm(Name = "Digits")] string selection,
            [FromQuery] string sectionOrGroupName)
        {
            if (selection == "#")
            {
                return TwilioRedirect(Routes.MainMenu);
            }

            if (selection == "0")
            {
                var group = _contentService.GetGroup(sectionOrGroupName);
                var parentName = (group == null || group.Parent == null) ? null : group.Parent.Name;
                return TwilioRedirect(GetBrowseUrl(parentName));
            }

            var contentUrl = _contentService.GetContentUrl(sectionOrGroupName);

            if (!string.IsNullOrEmpty(contentUrl))
            {
                var sectionName = sectionOrGroupName;
                SetUserCurrentSection(fromPhoneNumber, sectionName);
                return RedirectToCurrentContent();
            }

            var groupName = sectionOrGroupName;
            var options = GetNavOptions(groupName);

            int selectedOptionNumber;
            if (int.TryParse(selection, out selectedOptionNumber))
            {
                var selectedOptionName = options.Skip(selectedOptionNumber - 1).FirstOrDefault();
                sectionOrGroupName = selectedOptionName ?? sectionOrGroupName;
            }

            return TwilioRedirect(GetBrowseUrl(sectionOrGroupName));
        }

        private IActionResult RedirectToCurrentContent()
        {
            return TwilioResponseResult(x =>
            {
                x.Say("One moment please.");
                x.Redirect(Routes.CurrentContent, "get");
            });
        }

        private IActionResult ListNavOptions(string groupName)
        {
            var options = GetNavOptions(groupName).ToList();

            if (options.Any())
            {
                var numDigits = Convert.ToInt32(Math.Floor(Math.Log10(options.Count))) + 1;
                return TwilioResponseResult(x =>
                {
                    x.BeginGather(new {
                        action = GetBrowseUrl(groupName),
                        numDigits = numDigits,
                        timeout = 4
                    });

                    var optionNumber = 1;
                    foreach (var option in options)
                    {
                        x.Say("Enter " + optionNumber++ + " for " + option + ".");
                    }

                    if (groupName != null)
                    {
                        x.Say("Press 0 to go back.");
                    }

                    x.Say("Press pound for main menu.");

                    x.EndGather();
                    x.Redirect();
                });
            }

            return TwilioResponseResult(x =>
            {
                x.Say("No content found.");
                x.Redirect(Routes.MainMenu, "get");
            });
        }

        private IEnumerable<string> GetNavOptions(string groupName)
        {
            var choices = Enumerable.Empty<string>();

            var groups = _contentService.GetChildGroups(groupName);
            if (groups.Any())
            {
                choices = groups.Select(x => x.Name);
            }
            else
            {
                var sections = _contentService.GetSectionsInGroup(groupName);
                if (sections.Any())
                {
                    choices = sections.Select(x => x.Name);
                }
            }

            return choices;
        }

        private void SetUserCurrentSection(string phoneNumber, string newSectionId)
        {
            var user = _userService.GetOrCreate(phoneNumber);
            SetUserCurrentSection(user, newSectionId);
        }

        private void SetUserCurrentSection(User user, string newSectionId)
        {
            user.CurrentSectionId = newSectionId;
            _userService.AddOrUpdate(user);
        }

        private string GetBrowseUrl(string sectionOrGroupName)
        {
            return string.Concat(
                Routes.Browse, "?sectionOrGroupName=", WebUtility.UrlEncode(sectionOrGroupName));
        }

        private IActionResult TwilioRedirect(string url)
        {
            return TwilioResponseResult(x => x.Redirect(url, "get"));
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
