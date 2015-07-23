using Garnet.Api.ActionResults;
using Garnet.Api.Extensions;
using Garnet.Api.Routes;
using Garnet.Domain.Entities;
using Garnet.Domain.Enums;
using Garnet.Domain.Services;
using Microsoft.AspNet.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Garnet.Api.Controllers
{
    [Route(TwilioVoiceRoutes.Root)]
    public class TwilioVoiceController : Controller
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly IBrowserFactory _browserFactory;
        private readonly IBibleMetadataService _bibleMetadataService;
        private readonly ITimeService _timeService;

        public TwilioVoiceController(IUserService userService, IBibleMetadataService bibleMetadataService, IContentService contentService, IBrowserFactory browserFactory, ITimeService timeService)
        {
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
            _contentService = contentService;
            _browserFactory = browserFactory;
            _timeService = timeService;
        }

        #region Start

        [Route(TwilioVoiceRoutes.Start)]
        [HttpGet]
        public IActionResult Start()
        {
            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { timeout = 1, finishOnKey = "" });
                x.AliceSay("For main menu, press pound anytime.");
                x.EndGather();
                x.Redirect(TwilioVoiceRoutes.CurrentContent, "get");
            });
        }

        [Route(TwilioVoiceRoutes.Start)]
        [HttpPost]
        public IActionResult HandleStartInput([FromForm(Name = "Digits")] string digits)
        {
            return new TwilioRedirectResult(digits == "#" ?
                TwilioVoiceRoutes.MainMenu :
                TwilioVoiceRoutes.CurrentContent);
        }

        #endregion

        #region Current Content

        [Route(TwilioVoiceRoutes.CurrentContent)]
        [HttpGet]
        public async Task<IActionResult> GetCurrentContent([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            var user = await _userService.GetByPhoneNumberOrCreateAsync(fromPhoneNumber);

            var getContentUrlTask = _contentService.GetContentUrlAsync(user);
            var getCopyrightInfoTask = _contentService.GetCopyrightInfoAsync(user);

            var contentUrl = await getContentUrlTask;
            var copyrightInfo = await getCopyrightInfoTask;

            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { timeout = 1, finishOnKey = "" });
                x.Play(contentUrl);
                if (!string.IsNullOrEmpty(copyrightInfo))
                {
                    x.AliceSay(string.Concat("Copyright: ", copyrightInfo));
                }
                x.EndGather();

                x.BeginGather(new { action = TwilioVoiceRoutes.NextContentRequest, timeout = 3, finishOnKey = "" });
                x.AliceSay("To advance to the next chapter, press 1.");
                x.EndGather();

                x.Redirect(TwilioVoiceRoutes.MainMenu, "get");
            });
        }

        [Route(TwilioVoiceRoutes.CurrentContent)]
        [HttpPost]
        public IActionResult InterruptCurrentContent()
        {
            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        [Route(TwilioVoiceRoutes.NextContentRequest)]
        [HttpPost]
        public async Task<IActionResult> AdvanceToNextContent(
            [FromForm(Name = "From")] string phoneNumber, 
            [FromForm(Name = "Digits")] string digits)
        {
            if (digits == "1")
            {
                await AdvanceToNextContent(phoneNumber);
                return new RedirectToCurrentContentResult();
            }

            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        #endregion

        #region Main Menu

        [Route(TwilioVoiceRoutes.MainMenu)]
        [HttpGet]
        public async Task<IActionResult> GetMainMenu([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            var user = await _userService.GetByPhoneNumberOrCreateAsync(fromPhoneNumber);
            var currentChapter = _bibleMetadataService.GetChapterByNumber(user.ChapterNumber);
            var nextChapter = _bibleMetadataService.GetChapterAfter(currentChapter);

            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { numDigits = 1 });
                x.AliceSay("Main Menu");
                x.AliceSay(string.Format("Press 1 to hear {0}.", currentChapter));
                x.AliceSay(string.Format("Press 2 to hear {0}.", nextChapter));
                x.AliceSay("Press 3 to choose a different chapter.");
                x.AliceSay("Press 4 to choose a translation.");
                x.AliceSay("Press 5 to set a daily text message reminder.");
                x.EndGather();
                x.Redirect(TwilioVoiceRoutes.MainMenu, "get");
            });
        }

        [Route(TwilioVoiceRoutes.MainMenu)]
        [HttpPost]
        public async Task<IActionResult> HandleMainMenuSelection(
            [FromForm(Name = "From")] string phoneNumber, 
            [FromForm(Name = "Digits")] string selection)
        {
            var redirectToContent = selection == "1" || selection == "2";
            var advanceToNextContent = selection == "2";

            if (advanceToNextContent)
            {
                await AdvanceToNextContent(phoneNumber);
            }

            if (redirectToContent)
            {
                return new RedirectToCurrentContentResult();
            }
            else if (selection == "3")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.Browse);
            }
            else if (selection == "4")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.TranslationMenu);
            }
            else if (selection == "5")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.ReminderTime);
            }
            else if (selection == "6")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.LocalTime);
            }

            // TODO: Prevent infinite loop.

            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        #endregion

        #region Browse

        [Route(TwilioVoiceRoutes.Browse)]
        [HttpGet]
        public async Task<IActionResult> Browse(
            [FromQuery(Name = "From")] string phoneNumber, 
            [FromQuery] string bookOrGroupName,
            [FromQuery] bool navigatingUp = false)
        {
            var browser = await CreateBrowser(phoneNumber, bookOrGroupName);
            if (browser != null)
            {
                return await browser.HandleBrowseAsync(phoneNumber, navigatingUp);
            }

            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        [Route(TwilioVoiceRoutes.Browse)]
        [HttpPost]
        public async Task<IActionResult> HandleBrowseSelection(
            [FromForm(Name = "From")] string fromPhoneNumber,
            [FromForm(Name = "Digits")] string selection,
            [FromQuery] string bookOrGroupName)
        {
            var browser = await CreateBrowser(fromPhoneNumber, bookOrGroupName);
            if (browser != null)
            {
                return await browser.HandleSelection(fromPhoneNumber, selection);
            }

            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        internal static string GetBrowseUrl(string bookOrGroupName, bool navigatingUp = false)
        {
            var url = string.Concat(
                TwilioVoiceRoutes.Browse, "?bookOrGroupName=", WebUtility.UrlEncode(bookOrGroupName));

            if (navigatingUp)
            {
                url = string.Concat(url, "&navigatingUp=true");
            }

            return url;
        }

        private async Task<IBrowser> CreateBrowser(string phoneNumber, string bookOrGroupName)
        {
            if (string.IsNullOrEmpty(bookOrGroupName))
            {
                var user = await _userService.GetByPhoneNumberAsync(phoneNumber);
                if (user != null)
                {
                    var chapter = _bibleMetadataService.GetChapterByNumber(user.ChapterNumber);
                    if (chapter.Book.NumberOfChapters == 1)
                    {
                        return _browserFactory.CreateGroupBrowser(chapter.Book.Group);
                    }
                    else
                    {
                        return _browserFactory.CreateBookBrowser(chapter.Book);
                    }
                }
            }

            var book = _bibleMetadataService.GetBook(bookOrGroupName);
            if (book != null)
            {
                return _browserFactory.CreateBookBrowser(book);
            }

            var group = _bibleMetadataService.GetGroup(bookOrGroupName);
            if (group != null)
            {
                return _browserFactory.CreateGroupBrowser(group);
            }

            return null;
        }

        #endregion

        #region Translation Menu

        [Route(TwilioVoiceRoutes.TranslationMenu)]
        [HttpGet]
        public async Task<IActionResult> TranslationMenu(
            [FromQuery(Name = "From")] string phoneNumber)
        {
            var userTask = _userService.GetByPhoneNumberOrCreateAsync(phoneNumber);
            var volumesTask = _contentService.GetAvailableVolumesAsync();

            var user = await userTask;
            var volumes = (await volumesTask).ToList();

            var selectedVolume = volumes.FirstOrDefault(x =>
                x.Code == user.AudioVolumeCode &&
                x.IsDramatic == user.IsDramaticAudioSelected);

            var numDigits = volumes.Count == 0 ?
                0 :
                Convert.ToInt32(Math.Floor(Math.Log10(volumes.Count))) + 1;

            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { numDigits = numDigits });

                if (selectedVolume != null)
                {
                    x.AliceSay("Your current selection is " + GetDescription(selectedVolume) + ".");
                }

                var optionNumber = 1;
                foreach (var volumeDescription in volumes.Select(GetDescription))
                {
                    x.AliceSay("For " + volumeDescription + ", press " + optionNumber++ + ".");
                }

                x.EndGather();
            });
        }

        [Route(TwilioVoiceRoutes.TranslationMenu)]
        [HttpPost]
        public async Task<IActionResult> TranslationMenu(
            [FromForm(Name = "From")] string phoneNumber,
            [FromForm(Name = "Digits")] string digits)
        {
            var userTask = _userService.GetByPhoneNumberOrCreateAsync(phoneNumber);
            var volumesTask = _contentService.GetAvailableVolumesAsync();

            var user = await userTask;
            var volumes = await volumesTask;

            int selectedOptionNumber;
            if (int.TryParse(digits, out selectedOptionNumber))
            {
                var newSelectedVolume = volumes.Skip(selectedOptionNumber - 1).FirstOrDefault();
                if (newSelectedVolume != null)
                {
                    user.AudioVolumeCode = newSelectedVolume.Code;
                    user.IsDramaticAudioSelected = newSelectedVolume.IsDramatic;
                    await _userService.AddOrUpdateAsync(user);

                    return new TwilioResponseResult(x =>
                    {
                        x.AliceSay("Selected " + GetDescription(newSelectedVolume));
                        x.Redirect(TwilioVoiceRoutes.MainMenu);
                    });
                }
            }

            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        private static string GetDescription(AudioVolume volume)
        {
            var stringBuilder = new StringBuilder();

            if (volume.IsDramatic)
            {
                stringBuilder.Append("dramatic ");
            }

            stringBuilder.Append(volume.VersionName);

            switch (volume.CollectionType)
            {
                case CollectionType.OldTestamentOnly:
                    stringBuilder.Append(" old testament");
                    break;
                case CollectionType.NewTestamentOnly:
                    stringBuilder.Append(" new testament");
                    break;
            }

            return stringBuilder.ToString();
        }

        #endregion

        #region Schedule

        #region Reminder Time

        [Route(TwilioVoiceRoutes.ReminderTime)]
        [HttpGet]
        public async Task<IActionResult> StartSetTime(
            [FromQuery(Name = "From")] string phoneNumber)
        {
            int? reminderTimeInMinutes = null;

            var user = await _userService.GetByPhoneNumberAsync(phoneNumber);
            if (user != null)
            {
                reminderTimeInMinutes = user.ReminderTimeInMinutes;
            }

            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { finishOnKey = "" });
                if (reminderTimeInMinutes != null)
                {
                    x.AliceSay(string.Format("Your reminder is set for {0}",
                        GetSpokenTimeOfDay(reminderTimeInMinutes.Value)));
                }
                x.AliceSay(@"
                    This feature is not yet implemented.
                    Enter your desired reminder time. 
                    For example, enter 8 3 0 for 8:30.
                    Enter 0 to stop reminders.
                    Press pound for main menu.");
                x.EndGather();
            });
        }

        [Route(TwilioVoiceRoutes.ReminderTime)]
        [HttpPost]
        public async Task<IActionResult> HandleStartSetTime(
            [FromForm(Name = "From")] string phoneNumber,
            [FromForm(Name = "Digits")] string digits)
        {
            if (digits == "#")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
            }

            var user = await _userService.GetByPhoneNumberOrCreateAsync(phoneNumber);

            if (digits == "0")
            {
                user.ReminderTimeInMinutes = null;
                await _userService.AddOrUpdateAsync(user);
                return new TwilioResponseResult(x =>
                {
                    x.AliceSay("No daily reminders will be sent.");
                    x.Redirect(TwilioVoiceRoutes.MainMenu, "get");
                });
            }

            TimeSpan timeOfDay;
            if (digits.Length > 0 && digits.Length <= 2)
            {
                int hours;
                if (int.TryParse(digits, out hours))
                {
                    timeOfDay = TimeSpan.FromHours(hours);
                    return await HandleValidSetTime(digits, user, timeOfDay);
                }
            }
            if (digits.Length > 2 && TimeSpan.TryParse(digits.Insert(digits.Length - 2, ":"), out timeOfDay))
            {
                return await HandleValidSetTime(digits, user, timeOfDay);
            }

            return DidNotUnderstandReminderSetResponse();
        }

        private async Task<IActionResult> HandleValidSetTime(string digits, User user, TimeSpan timeOfDay)
        {
            if (timeOfDay.Hours > 0 && timeOfDay.Hours <= 12)
            {
                return new TwilioRedirectResult(
                    TwilioVoiceRoutes.ClarifyReminderTime + "?time=" + WebUtility.UrlEncode(digits));
            }
            else
            {
                user.ReminderTimeInMinutes = (int)timeOfDay.TotalMinutes;
                await _userService.AddOrUpdateAsync(user);
                return ReminderSetResponse(user.ReminderTimeInMinutes.Value);
            }
        }

        #endregion

        #region Clarify AM/PM

        [Route(TwilioVoiceRoutes.ClarifyReminderTime)]
        [HttpGet]
        public IActionResult ClarifyReminderTime(
            [FromQuery(Name = "From")] string phoneNumber,
            [FromQuery] string time)
        {
            TimeSpan timeOfDay;
            if (time.Length > 2 && TimeSpan.TryParse(time.Insert(time.Length - 2, ":"), out timeOfDay))
            {
                var spokenTimeOfDay = GetSpokenTimeOfDay((int)timeOfDay.TotalMinutes, includeAmPm: false);
                return new TwilioResponseResult(x =>
                {
                    x.BeginGather(new
                    {
                        action = string.Concat(TwilioVoiceRoutes.ClarifyReminderTime, "?time=", WebUtility.UrlEncode(time)),
                        finishOnKey = ""
                    });
                    x.AliceSay(string.Format("Press 1 for {0} A.M.", spokenTimeOfDay));
                    x.AliceSay(string.Format("Press 2 for {0} P.M.", spokenTimeOfDay));
                    x.EndGather();
                    x.Redirect(TwilioVoiceRoutes.ReminderTime, "get");
                });
            }

            return new TwilioRedirectResult(TwilioVoiceRoutes.ReminderTime);
        }

        [Route(TwilioVoiceRoutes.ClarifyReminderTime)]
        [HttpPost]
        public async Task<IActionResult> HandleClarifyReminderTime(
            [FromForm(Name = "From")] string phoneNumber,
            [FromQuery] string time,
            [FromForm(Name = "Digits")] string digits)
        {
            var isPM = false;
            if (digits == "2")
            {
                isPM = true;
            }
            else if (digits != "1")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.ReminderTime);
            }

            TimeSpan timeOfDay;
            if (time.Length > 2 && TimeSpan.TryParse(time.Insert(time.Length - 2, ":"), out timeOfDay))
            {
                if (isPM && timeOfDay.Hours < 12)
                {
                    timeOfDay = timeOfDay.Add(TimeSpan.FromHours(12));
                }
                else if (!isPM && timeOfDay.Hours == 12)
                {
                    timeOfDay = timeOfDay.Subtract(TimeSpan.FromHours(12));
                }

                var user = await _userService.GetByPhoneNumberOrCreateAsync(phoneNumber);
                user.ReminderTimeInMinutes = (int)timeOfDay.TotalMinutes;
                await _userService.AddOrUpdateAsync(user);
                return ReminderSetResponse(user.ReminderTimeInMinutes.Value);
            }

            return DidNotUnderstandReminderSetResponse();
        }

        #endregion

        #region Get Local Time

        [Route(TwilioVoiceRoutes.LocalTime)]
        [HttpGet]
        public async Task<IActionResult> GetCallerLocalTime(
            [FromQuery(Name = "FromCity")] string fromCity,
            [FromQuery(Name = "FromState")] string fromState,
            [FromQuery(Name = "FromZip")] string fromZip
            )
        {
            var localTime = await _timeService.GetLocalTimeAsync(fromCity, fromState, fromZip);
            if (localTime != null)
            {
                return new TwilioResponseResult(x =>
                {
                    x.BeginGather(new { numDigits = 1 });
                    x.AliceSay(string.Format("Is your local time {0}?", GetSpokenTimeOfDay(localTime.Value)));
                    x.AliceSay("If this is correct, press 1.");
                    x.AliceSay("If this is incorrect, press 2.");
                    x.EndGather();
                });
            }

            // TODO: Prompt user to enter local time.
            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        [Route(TwilioVoiceRoutes.LocalTime)]
        [HttpPost]
        public IActionResult HandleCallerLocalTime()
        {
            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }

        #endregion

        private static TwilioResponseResult DidNotUnderstandReminderSetResponse()
        {
            return new TwilioResponseResult(x =>
            {
                x.AliceSay("I didn't understand.");
                x.Redirect(TwilioVoiceRoutes.ReminderTime, "get");
            });
        }

        private static TwilioResponseResult ReminderSetResponse(int reminderTimeInMinutes)
        {
            return new TwilioResponseResult(x =>
            {
                x.AliceSay(string.Concat("Daily reminder set for ",
                    GetSpokenTimeOfDay(reminderTimeInMinutes)));
                x.Redirect(TwilioVoiceRoutes.MainMenu);
            });
        }

        private static string GetSpokenTimeOfDay(int reminderTimeInMinutes, bool includeAmPm = true)
        {
            return GetSpokenTimeOfDay(TimeSpan.FromMinutes(reminderTimeInMinutes), includeAmPm);
        }

        private static string GetSpokenTimeOfDay(TimeSpan timeOfDay, bool includeAmPm = true)
        {
            var anteMeridianOrPostMeridian = timeOfDay.TotalHours < 12 ? "A.M." : "P.M.";
            if (timeOfDay.TotalHours < 1)
            {
                timeOfDay = timeOfDay.Add(TimeSpan.FromHours(12));
            }
            else if (timeOfDay.TotalHours >= 13)
            {
                timeOfDay = timeOfDay.Subtract(TimeSpan.FromHours(12));
            }

            var segments = new List<string>();
            segments.Add(timeOfDay.Minutes == 0 ?
                timeOfDay.Hours.ToString() :
                timeOfDay.ToString("h':'mm"));
            if (includeAmPm)
            {
                segments.Add(anteMeridianOrPostMeridian);
            }
            return string.Join(" ", segments);
        }

        #endregion

        private async Task AdvanceToNextContent(string phoneNumber)
        {
            var user = await _userService.GetByPhoneNumberOrCreateAsync(phoneNumber);
            user.ChapterNumber++;
            if (_bibleMetadataService.GetChapterByNumber(user.ChapterNumber) == null)
            {
                user.ChapterNumber = 1;
            }
            await _userService.AddOrUpdateAsync(user);
        }
    }
}
