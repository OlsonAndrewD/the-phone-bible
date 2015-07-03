using Garnet.Api.ActionResults;
using Garnet.Api.Extensions;
using Garnet.Api.Routes;
using Garnet.Api.TwilioRequestHandlers;
using Garnet.Domain.Entities;
using Garnet.Domain.Enums;
using Garnet.Domain.Services;
using Microsoft.AspNet.Mvc;
using System;
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
        private readonly MainMenu _mainMenu;
        private readonly IBrowserFactory _browserFactory;
        private readonly IBibleMetadataService _bibleMetadataService;

        public TwilioVoiceController(IUserService userService, IBibleMetadataService bibleMetadataService, IContentService contentService, MainMenu mainMenu, IBrowserFactory browserFactory)
        {
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
            _contentService = contentService;
            _mainMenu = mainMenu;
            _browserFactory = browserFactory;
        }

        [Route(TwilioVoiceRoutes.Start)]
        [HttpGet]
        public IActionResult Start()
        {
            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { timeout = 2, finishOnKey = '*' /* just so # will POST instead of falling through */ });
                x.AliceSay("For main menu, press pound anytime.");
                x.EndGather();
                x.Redirect(TwilioVoiceRoutes.CurrentContent, "get");
            });
        }

        [Route(TwilioVoiceRoutes.Start)]
        [HttpPost]
        public IActionResult HandleStartInput([FromForm(Name = "Digits")] string digits)
        {
            return new TwilioRedirectResult(digits == "#" ? TwilioVoiceRoutes.MainMenu : TwilioVoiceRoutes.CurrentContent);
        }

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
                x.BeginGather(new { timeout = 2, finishOnKey = '*' /* just so # will POST instead of falling through */ });
                x.Play(contentUrl);
                if (!string.IsNullOrEmpty(copyrightInfo))
                {
                    x.AliceSay(string.Concat("Copyright: ", copyrightInfo));
                }
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

        [Route(TwilioVoiceRoutes.MainMenu)]
        [HttpGet]
        public async Task<IActionResult> GetMainMenu([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            return await _mainMenu.Get(fromPhoneNumber);
        }

        [Route(TwilioVoiceRoutes.MainMenu)]
        [HttpPost]
        public async Task<IActionResult> HandleMainMenuSelection(
            [FromForm(Name = "From")] string phoneNumber, 
            [FromForm(Name = "Digits")] string selection)
        {
            return await _mainMenu.HandleSelection(phoneNumber, selection);
        }

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
    }
}
