﻿using Garnet.Api.ActionResults;
using Twilio.TwiML;
using Garnet.Domain.Services;
using Garnet.Domain.Entities;

namespace Garnet.Api.TwilioRequestHandlers
{
    public class BookBrowser : Browser
    {
        private readonly IUserService _userService;
        private readonly IContentService _contentService;
        private readonly Book _book;

        public BookBrowser(Book book, IUserService userService, IContentService contentService)
        {
            _book = book;
            _userService = userService;
            _contentService = contentService;
        }

        protected override string Name
        {
            get { return _book.Name; }
        }

        protected override string ParentName
        {
            get { return _book.Group.Name; }
        }

        protected override int NumberOfOptions
        {
            get { return _book.NumberOfChapters; }
        }

        protected override void PromptForSelectionInternal(TwilioResponse response)
        {
            response.Say(string.Concat("Enter a chapter number between 1 and ", _book.NumberOfChapters, "."));
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
                        BookName = ParentName,
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
