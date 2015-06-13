using System.Collections.Generic;
using Garnet.Api.TwilioRequestHandlers;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using System.Linq;

namespace Garnet.Api
{
    public class BrowserFactory : IBrowserFactory
    {
        private readonly IContentService _contentService;
        private readonly IUserService _userService;

        public BrowserFactory(IUserService userService, IContentService contentService)
        {
            _userService = userService;
            _contentService = contentService;
        }

        public IBrowser CreateBookBrowser(Book book)
        {
            return new BookBrowser(book, _userService, _contentService);
        }

        public IBrowser CreateGroupBrowser(BookGroup bookGroup)
        {
            var childGroups = _contentService.GetChildGroups(bookGroup.Name);
            if (childGroups.Any())
            {
                return new GroupBrowser(bookGroup, childGroups.Select(x => x.Name));
            }
            else
            {
                var books = _contentService.GetBooks(bookGroup.Name);
                return new GroupBrowser(bookGroup, books.Select(x => x.Name));
            }
        }
    }
}
