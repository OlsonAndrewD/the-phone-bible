using Garnet.Api.TwilioRequestHandlers.Browsers.OldTestament;
using System.Collections.Generic;

namespace Garnet.Api.TwilioRequestHandlers.Browsers
{
    public class OldTestamentBrowser : GroupBrowser
    {
        public OldTestamentBrowser()
            : base(GetGroups())
        {
        }

        public const string Name = "Old Testament";

        protected override string GroupName
        {
            get { return Name; }
        }

        protected override string ParentGroupName
        {
            get { return TopLevelBrowser.Name; }
        }

        private static IEnumerable<string> GetGroups()
        {
            yield return BooksOfMosesBrowser.Name;
            yield return "Historical Books";
            yield return "Wisdom and Poetry";
            yield return "Major Prophets";
            yield return "Minor Prophets";
        }
    }
}
