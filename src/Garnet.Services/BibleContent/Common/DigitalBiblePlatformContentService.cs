using Garnet.Domain.Entities;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
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
            if (apiKey == null)
            {
                throw new ArgumentNullException("apiKey");
            }
            _apiKey = apiKey;
        }

        protected abstract string LanguageCode { get; }
        protected abstract string VersionCode { get; }
        protected abstract bool IsDramaType { get; }

        public Task<string> GetContentUrlAsync(Chapter chapter)
        {
            return Task.WhenAll(GetBaseAudioUrl(), GetAudioUrl(chapter))
                .ContinueWith(x => string.Join("/", x.Result));
        }

        private async Task<string> GetBaseAudioUrl()
        {
            var restClient = CreateDbtRestClient();

            var response = await restClient.ExecuteGetTaskAsync<List<AudioLocation>>(new RestRequest("location"));
            var location = response.Data.OrderBy(x => x.Priority).FirstOrDefault();

            if (location == null)
            {
                throw new Exception("Unable to determine base audio URL.");
            }

            var uriBuilder = new UriBuilder
            {
                Scheme = location.Protocol,
                Host = location.Server,
                Path = location.RootPath
            };
            return uriBuilder.ToString();
        }

        private async Task<string> GetAudioUrl(Chapter chapter)
        {
            var restClient = CreateDbtRestClient();
            var request = new RestRequest("path");
            request.AddParameter("dam_id", GetDamId(chapter));
            request.AddParameter("book_id", chapter.BookName);
            request.AddParameter("chapter_id", chapter.ChapterNumber);

            var response = await restClient.ExecuteGetTaskAsync<List<AudioPath>>(request);
            var path = response.Data.FirstOrDefault();

            if (path == null)
            {
                throw new Exception("Unable to determine audio URL.");
            }

            return path.Path;
        }

        private string GetDamId(Chapter chapter)
        {
            return string.Concat(
                LanguageCode,
                VersionCode,
                GetCollectionId(chapter),
                IsDramaType ? '2' : '1',
                MediaType);
        }

        private RestClient CreateDbtRestClient()
        {
            var restClient = new RestClient("http://dbt.io/audio/");
            restClient.AddDefaultParameter("key", _apiKey);
            restClient.AddDefaultParameter("v", ApiVersion);
            return restClient;
        }

        public class AudioLocation
        {
            public string Protocol { get; set; }
            public string Server { get; set; }
            public string RootPath { get; set; }
            public int Priority { get; set; }
        }

        public class AudioPath
        {
            public string Path { get; set; }
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
