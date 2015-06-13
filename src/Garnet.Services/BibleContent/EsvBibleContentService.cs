using System;
using Garnet.Domain.Services;
using Garnet.Services.BibleContent.Common;

namespace Garnet.Services.BibleContent
{
    public class EsvBibleContentService : DigitalBiblePlatformContentService, IContentService
    {
        public EsvBibleContentService(string apiKey) : base(apiKey)
        {
        }

        protected override bool IsDramaType
        {
            get { return true; }
        }

        protected override string LanguageCode
        {
            get { return "ENG"; }
        }

        protected override string VersionCode
        {
            get { return "ESV"; }
        }
    }
}
