using Garnet.Domain.Services;

namespace Garnet.Services.BibleContentServices
{
    public class KjvBibleContentService : DigitalBiblePlatformContentService, IContentService
    {
        public KjvBibleContentService(IUserService userService, IBibleMetadataService bibleMetadataService, string apiKey)
            : base(userService, bibleMetadataService, apiKey)
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
