using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garnet.Api.TwilioRequestHandlers.Browsers
{
    public class TopLevelBrowser : GroupBrowser
    {
        public TopLevelBrowser()
            : base(GetGroups())
        {
        }

        public const string Name = "Top";

        protected override string GroupName
        {
            get { return Name; }
        }

        protected override string ParentGroupName
        {
            get { return null; }
        }

        private static IEnumerable<string> GetGroups()
        {
            yield return OldTestamentBrowser.Name;
            yield return NewTestamentBrowser.Name;
        }
    }
}
