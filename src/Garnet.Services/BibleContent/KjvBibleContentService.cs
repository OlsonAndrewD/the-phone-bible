using Garnet.Domain.Services;
using Garnet.Services.BibleContent.Common;

namespace Garnet.Services.BibleContent
{
    public class KjvBibleContentService : DigitalBiblePlatformContentService, IContentService
    {
        public KjvBibleContentService(string apiKey) : base(apiKey)
        {
        }

        protected override bool IsDramaType
        {
            get { return false; }
        }

        protected override string LanguageCode
        {
            get { return "ENG"; }
        }

        protected override string VersionCode
        {
            get { return "KJV"; }
        }
    }
}
