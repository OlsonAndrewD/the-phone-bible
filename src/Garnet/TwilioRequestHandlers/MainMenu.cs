﻿using Garnet.Api.ActionResults;
using Garnet.Domain.Services;

namespace Garnet.Api.TwilioRequestHandlers
{
    public class MainMenu
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;

        public MainMenu(IUserService userService, IContentService contentService)
        {
            _userService = userService;
            _contentService = contentService;
        }

        public TwilioResponseResult Get()
        {
            return new TwilioResponseResult(x =>
            {
                x.BeginGather(new { numDigits = 1 });
                x.Say(string.Concat("Press 1 to hear the current chapter."));
                x.Say(string.Concat("Press 2 to hear the next chapter."));
                x.Say(string.Concat("Press 3 to choose a different chapter."));
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
                user.CurrentChapter = _contentService.GetChapterAfter(user.CurrentChapter);
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