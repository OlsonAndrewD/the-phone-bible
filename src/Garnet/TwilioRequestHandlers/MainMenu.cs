using Garnet.Api.ActionResults;
using Garnet.Api.Extensions;
using Garnet.Domain.Services;

namespace Garnet.Api.TwilioRequestHandlers
{
    public class MainMenu
    {
        private readonly IUserService _userService;
        private readonly IBibleMetadataService _bibleMetadataService;

        public MainMenu(IUserService userService, IBibleMetadataService bibleMetadataService)
        {
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
        }

        public TwilioResponseResult Get()
        {
            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { numDigits = 1 });
                x.AliceSay(string.Concat("Press 1 to hear the current chapter."));
                x.AliceSay(string.Concat("Press 2 to hear the next chapter."));
                x.AliceSay(string.Concat("Press 3 to choose a different chapter."));
                x.EndGather();
                x.Redirect(Routes.MainMenu, "get");
            });
        }

        public TwilioResponseResult HandleSelection(string phoneNumber, string selection)
        {
            var redirectToContent = selection == "1" || selection == "2";
            var advanceToNextContent = selection == "2";

            if (advanceToNextContent)
            {
                var user = _userService.GetOrCreate(phoneNumber);
                user.CurrentChapterNumber++;
                if (_bibleMetadataService.GetChapterByNumber(user.CurrentChapterNumber) == null)
                {
                    user.CurrentChapterNumber = 1;
                }
                _userService.AddOrUpdate(user);
            }

            if (redirectToContent)
            {
                return new RedirectToCurrentContent();
            }
            else if (selection == "3")
            {
                return new TwilioRedirectResult(Routes.Browse);
            }

            // TODO: Prevent infinite loop.
            return new TwilioRedirectResult(Routes.MainMenu);
        }
    }
}
