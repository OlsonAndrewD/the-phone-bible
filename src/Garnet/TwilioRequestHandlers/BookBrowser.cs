using Garnet.Api.ActionResults;
using Twilio.TwiML;
using Garnet.Domain.Services;
using Garnet.Domain.Entities;

namespace Garnet.Api.TwilioRequestHandlers
{
    public abstract class BookBrowser : Browser
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;

        public BookBrowser(IUserService userService, IContentService contentService)
        {
            _userService = userService;
            _contentService = contentService;
        }

        protected override void PromptForSelectionInternal(TwilioResponse response)
        {
            response.Say(string.Concat("Enter a chapter number between 1 and ", NumberOfOptions, "."));
        }

        protected override TwilioResponseResult HandleSelectionInternal(string phoneNumber, string selection)
        {
            var user = _userService.GetOrCreate(phoneNumber);
            if (user != null)
            {
                int chapterNumber;
                if (int.TryParse(selection, out chapterNumber))
                {
                    user.CurrentChapter = new Chapter
                    {
                        BookName = ParentGroupName,
                        ChapterNumber = int.Parse(selection)
                    };
                    _userService.AddOrUpdate(user);
                    return new RedirectToCurrentContent();
                }
            }
            return new TwilioRedirectResult();
        }
    }
}
