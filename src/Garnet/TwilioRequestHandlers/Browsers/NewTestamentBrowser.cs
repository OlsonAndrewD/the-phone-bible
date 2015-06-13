using System.Collections.Generic;

namespace Garnet.Api.TwilioRequestHandlers.Browsers
{
    public class NewTestamentBrowser : GroupBrowser
    {
        public NewTestamentBrowser()
            : base(GetGroups())
        {
        }

        public const string Name = "New Testament";

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
            yield return "Gospels and Acts";
            yield return "Paul's Letters";
            yield return "General Letters and Prophesy";
        }
    }
}
