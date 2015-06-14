using Garnet.Api.ActionResults;
using Garnet.Api.Extensions;
using Garnet.Api.TwilioRequestHandlers;
using Garnet.Domain.Services;
using Microsoft.AspNet.Mvc;
using System.Net;
using System.Threading.Tasks;

namespace Garnet.Api.Controllers
{
    [Route(Routes.Root)]
    public class TwilioVoiceController : Controller
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly MainMenu _mainMenu;
        private readonly IBrowserFactory _browserFactory;

        public TwilioVoiceController(IUserService userService, IContentService contentService, MainMenu mainMenu, IBrowserFactory browserFactory)
        {
            _userService = userService;
            _contentService = contentService;
            _mainMenu = mainMenu;
            _browserFactory = browserFactory;
        }

        [Route(Routes.Start)]
        [HttpGet]
        public IActionResult Start()
        {
            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.AliceSay("For main menu, press pound anytime.");
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
        public async Task<IActionResult> GetCurrentContent([FromQuery(Name = "From")] string fromPhoneNumber)
        {
            var user = _userService.GetOrCreate(fromPhoneNumber);

            var getContentUrlTask = _contentService.GetContentUrlAsync(user.CurrentChapter);
            var getCopyrightInfoTask = _contentService.GetCopyrightInfoAsync(user.CurrentChapter);

            var contentUrl = await getContentUrlTask;
            var copyrightInfo = await getCopyrightInfoTask;

            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { action = Routes.MainMenu, timeout = 2 });
                x.Play(contentUrl);
                if (!string.IsNullOrEmpty(copyrightInfo))
                {
                    x.AliceSay(string.Concat("Copyright: ", copyrightInfo));
                }
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
        public IActionResult Browse(
            [FromQuery(Name = "From")] string phoneNumber, 
            [FromQuery] string bookOrGroupName,
            [FromQuery] bool navigatingUp = false)
        {
            var browser = CreateBrowser(phoneNumber, bookOrGroupName);
            if (browser != null)
            {
                return browser.HandleBrowse(phoneNumber, navigatingUp);
            }

            return new TwilioRedirectResult(Routes.MainMenu);
        }

        [Route(Routes.Browse)]
        [HttpPost]
        public IActionResult HandleBrowseSelection(
            [FromForm(Name = "From")] string fromPhoneNumber,
            [FromForm(Name = "Digits")] string selection,
            [FromQuery] string bookOrGroupName)
        {
            var browser = CreateBrowser(fromPhoneNumber, bookOrGroupName);
            if (browser != null)
            {
                return browser.HandleSelection(fromPhoneNumber, selection);
            }

            return new TwilioRedirectResult(Routes.MainMenu);
        }

        internal static string GetBrowseUrl(string bookOrGroupName, bool navigatingUp = false)
        {
            var url = string.Concat(
                Routes.Browse, "?bookOrGroupName=", WebUtility.UrlEncode(bookOrGroupName));

            if (navigatingUp)
            {
                url = string.Concat(url, "&navigatingUp=true");
            }

            return url;
        }

        private IBrowser CreateBrowser(string phoneNumber, string bookOrGroupName)
        {
            if (string.IsNullOrEmpty(bookOrGroupName))
            {
                var user = _userService.Get(phoneNumber);
                if (user != null)
                {
                    if (user.CurrentChapter.Book.NumberOfChapters == 1)
                    {
                        return _browserFactory.CreateGroupBrowser(user.CurrentChapter.Book.Group);
                    }
                    else
                    {
                        return _browserFactory.CreateBookBrowser(user.CurrentChapter.Book);
                    }
                }
            }

            var book = _contentService.GetBook(bookOrGroupName);
            if (book != null)
            {
                return _browserFactory.CreateBookBrowser(book);
            }

            var group = _contentService.GetGroup(bookOrGroupName);
            if (group != null)
            {
                return _browserFactory.CreateGroupBrowser(group);
            }

            return null;
        }
    }
}
