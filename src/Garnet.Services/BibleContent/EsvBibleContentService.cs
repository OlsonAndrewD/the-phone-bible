using System;
using Garnet.Domain.Services;
using Garnet.Services.BibleContent.Common;

namespace Garnet.Services.BibleContent
{
    public class EsvBibleContentService : DigitalBiblePlatformContentService, IContentService
    {
        public EsvBibleContentService() : base("")
        {
        }

        protected override bool IsDramaType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override string LanguageCode
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        protected override string VersionCode
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}
