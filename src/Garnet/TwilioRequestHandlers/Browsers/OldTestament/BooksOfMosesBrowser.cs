using System.Collections.Generic;

namespace Garnet.Api.TwilioRequestHandlers.Browsers.OldTestament
{
    public class BooksOfMosesBrowser : GroupBrowser
    {
        public BooksOfMosesBrowser()
            : base(GetGroups())
        {
        }

        public const string Name = "Books of Moses";

        protected override string GroupName
        {
            get { return Name; }
        }

        protected override string ParentGroupName
        {
            get { return OldTestamentBrowser.Name; }
        }

        private static IEnumerable<string> GetGroups()
        {
            yield return "Genesis";
            yield return "Exodus";
            yield return "Leviticus";
            yield return "Numbers";
            yield return "Deuteronomy";
        }
    }
}
