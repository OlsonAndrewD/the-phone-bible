using Garnet.Api.ActionResults;
using Garnet.Api.Extensions;
using Garnet.Api.Routes;
using Garnet.Domain.Services;
using System.Threading.Tasks;

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
                x.Redirect(TwilioVoiceRoutes.MainMenu, "get");
            });
        }

        public async Task<TwilioResponseResult> HandleSelection(string phoneNumber, string selection)
        {
            var redirectToContent = selection == "1" || selection == "2";
            var advanceToNextContent = selection == "2";

            if (advanceToNextContent)
            {
                var user = await _userService.GetOrCreateAsync(phoneNumber);
                user.CurrentChapterNumber++;
                if (_bibleMetadataService.GetChapterByNumber(user.CurrentChapterNumber) == null)
                {
                    user.CurrentChapterNumber = 1;
                }
                await _userService.AddOrUpdateAsync(user);
            }

            if (redirectToContent)
            {
                return new RedirectToCurrentContent();
            }
            else if (selection == "3")
            {
                return new TwilioRedirectResult(TwilioVoiceRoutes.Browse);
            }

            // TODO: Prevent infinite loop.
            return new TwilioRedirectResult(TwilioVoiceRoutes.MainMenu);
        }
    }
}
