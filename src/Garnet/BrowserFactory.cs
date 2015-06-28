using System.Collections.Generic;
using Garnet.Api.TwilioRequestHandlers;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using System.Linq;

namespace Garnet.Api
{
    public class BrowserFactory : IBrowserFactory
    {
        private readonly IUserService _userService;
        private readonly IBibleMetadataService _bibleMetadataService;

        public BrowserFactory(IUserService userService, IBibleMetadataService bibleMetadataService)
        {
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
        }

        public IBrowser CreateBookBrowser(Book book)
        {
            return new BookBrowser(book, _userService, _bibleMetadataService);
        }

        public IBrowser CreateGroupBrowser(BookGroup bookGroup)
        {
            var childGroups = _bibleMetadataService.GetChildGroups(bookGroup.Name);
            if (childGroups.Any())
            {
                return new GroupBrowser(bookGroup, childGroups.Select(x => x.Name));
            }
            else
            {
                var books = _bibleMetadataService.GetBooks(bookGroup.Name);
                return new GroupBrowser(bookGroup, books.Select(x => x.Name));
            }
        }
    }
}
