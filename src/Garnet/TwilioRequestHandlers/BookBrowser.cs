using Garnet.Api.ActionResults;
using Twilio.TwiML;
using Garnet.Domain.Services;
using Garnet.Domain.Entities;
using Garnet.Domain.Extensions;
using System;
using Garnet.Api.Extensions;

namespace Garnet.Api.TwilioRequestHandlers
{
    public class BookBrowser : Browser
    {
        private readonly IUserService _userService;
        private readonly Book _book;
        private readonly IBibleMetadataService _bibleMetadataService;

        public BookBrowser(Book book, IUserService userService, IBibleMetadataService bibleMetadataService)
            : base(book.Group.GetTopmostAncestor().Name)
        {
            _book = book;
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
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

        protected override void HandleBrowseInternal(TwilioResponse response)
        {
            response.AliceSay(
                string.Concat("Enter a chapter number between 1 and ", _book.NumberOfChapters, "."));
        }

        protected override TwilioResponseResult HandleSelectionInternal(string phoneNumber, string selection)
        {
            var user = _userService.GetOrCreate(phoneNumber);
            if (user != null)
            {
                int chapterNumber;
                if (int.TryParse(selection, out chapterNumber))
                {
                    user.CurrentChapterNumber = _bibleMetadataService.GetChapterNumber(
                        new Chapter
                        {
                            Book = _book,
                            ChapterNumber = int.Parse(selection)
                        });
                    _userService.AddOrUpdate(user);
                    return new RedirectToCurrentContent();
                }
            }
            return new TwilioRedirectResult();
        }
    }
}
