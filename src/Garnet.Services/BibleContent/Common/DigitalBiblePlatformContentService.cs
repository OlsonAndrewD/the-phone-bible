using Garnet.Domain.Entities;
using System;
using System.Threading.Tasks;

namespace Garnet.Services.BibleContent.Common
{
    public abstract class DigitalBiblePlatformContentService : BibleContentService
    {
        private readonly string _apiKey;
        private const string MediaType = "DA"; // only digital audio for now
        private const char ApiVersion = '2';

        public DigitalBiblePlatformContentService(string apiKey)
        {
            _apiKey = apiKey;
        }

        protected abstract string LanguageCode { get; }
        protected abstract string VersionCode { get; }
        protected abstract bool IsDramaType { get; }

        public Task<string> GetContentUrlAsync(Chapter chapter)
        {
            //var digitalAssetManagementId = string.Concat(
            //    LanguageCode,
            //    VersionCode,
            //    GetCollectionId(chapter),
            //    IsDramaType ? '2' : '1',
            //    MediaType);

            return Task.FromResult("http://cloud.faithcomesbyhearing.com/mp3audiobibles2/ENGESVN2DA/B04___01_John________ENGESVN2DA.mp3");
        }

        private char GetCollectionId(Chapter chapter)
        {
            return GetCollectionId(GetBook(chapter.BookName).Group);
        }

        private char GetCollectionId(BookGroup group)
        {
            if (group.Parent == EntireBible)
            {
                if (group == OldTestament)
                {
                    return 'O';
                }
                if (group == NewTestament)
                {
                    return 'N';
                }
                throw new Exception(string.Concat("Unexpected book group: ", group.Name));
            }

            return GetCollectionId(group.Parent);
        }
    }
}
