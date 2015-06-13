using System;
using Garnet.Domain.Services;

namespace Garnet.Api.TwilioRequestHandlers.Browsers.OldTestament.BooksOfMoses
{
    public class GenesisBrowser : BookBrowser
    {
        public GenesisBrowser(IUserService userService, IContentService contentService) : base(userService, contentService)
        {
        }

        public const string Name = "Genesis";

        protected override string GroupName
        {
            get { return Name; }
        }

        protected override int NumberOfOptions
        {
            get { return 50; }
        }

        protected override string ParentGroupName
        {
            get { return BooksOfMosesBrowser.Name; }
        }
    }
}
