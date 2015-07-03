using Garnet.Domain.Entities;
using Garnet.Domain.Enums;
using Garnet.Domain.Services;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Garnet.Services.BibleContentServices
{
    public abstract class DigitalBiblePlatformContentService
    {
        private readonly string _apiKey;
        private const string MediaType = "DA"; // only digital audio for now
        private const char ApiVersion = '2';
        private readonly IUserService _userService;
        private readonly IBibleMetadataService _bibleMetadataService;

        public DigitalBiblePlatformContentService(IUserService userService, IBibleMetadataService bibleMetadataService, string apiKey)
        {
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
            if (apiKey == null)
            {
                throw new ArgumentNullException("apiKey");
            }
            _apiKey = apiKey;
        }

        protected abstract string LanguageCode { get; }
        protected abstract string VersionCode { get; }
        protected abstract bool IsDramaType { get; }

        public async Task<IEnumerable<AudioVolume>> GetAvailableVolumesAsync()
        {
            var restClient = CreateDbtRestClient();
            var request = new RestRequest("library/volume");
            request.AddParameter("media", "audio");
            request.AddParameter("delivery", "mobile");
            request.AddParameter("language_family_code", "ENG");

            var response = await restClient.ExecuteGetTaskAsync<List<Volume>>(request);
            return response.Data
                .Select(x => new
                {
                    VersionName = x.VersionName.Trim(),
                    IsDramatic = (x.MediaType ?? string.Empty).ToLowerInvariant() == "drama",
                    CollectionType = x.DamId[6]
                })
                .GroupBy(x => new { x.VersionName, x.IsDramatic })
                .Where(x => x.Any(y => y.CollectionType == 'O' || y.CollectionType == 'N'))
                .Select(x =>
                {
                    CollectionType collectionType = CollectionType.Complete;
                    var includesOldTestament = x.Any(y => y.CollectionType == 'O');
                    var includesNewTestament = x.Any(y => y.CollectionType == 'N');
                    if (includesOldTestament ^ includesNewTestament)
                    {
                        collectionType = includesNewTestament ?
                            CollectionType.NewTestamentOnly :
                            CollectionType.OldTestamentOnly;
                    }

                    return new AudioVolume
                    {
                        VersionName = x.Key.VersionName,
                        IsDramatic = x.Key.IsDramatic,
                        CollectionType = collectionType
                    };
                });
        }

        public class Volume
        {
            public string DamId { get; set; }
            public string VersionName { get; set; }
            public string MediaType { get; set; }
        }

        public Task<string> GetContentUrlAsync(User user)
        {
            var chapter = _bibleMetadataService.GetChapterByNumber(user.ChapterNumber);
            return Task.WhenAll(GetBaseAudioUrl(), GetAudioUrl(chapter))
                .ContinueWith(x => string.Join("/", x.Result));
        }

        public async Task<string> GetCopyrightInfoAsync(User user)
        {
            var chapter = _bibleMetadataService.GetChapterByNumber(user.ChapterNumber);

            var restClient = CreateDbtRestClient();
            var request = new RestRequest("library/metadata");
            request.AddParameter("dam_id", GetDamId(chapter));

            var response = await restClient.ExecuteGetTaskAsync<List<CopyrightInfo>>(request);
            var copyrightInfo = response.Data.FirstOrDefault();

            string text = null;
            if (copyrightInfo != null)
            {
                text = copyrightInfo.Mark;
                if (string.IsNullOrEmpty(text))
                {
                    var holder = copyrightInfo.Organization.FirstOrDefault(x => x.OrganizationRole == "holder");
                    if (holder != null)
                    {
                        text = holder.Organization;
                    }
                }
            }
            return text;
        }

        public class CopyrightInfo
        {
            public string Mark { get; set; }
            public List<OrganizationInfo> Organization { get; set; }

            public class OrganizationInfo
            {
                public string OrganizationRole { get; set; }
                public string Organization { get; set; }
            }
        }

        private async Task<string> GetBaseAudioUrl()
        {
            var restClient = CreateDbtRestClient();

            var response = await restClient.ExecuteGetTaskAsync<List<AudioLocation>>(new RestRequest("audio/location"));
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
            var request = new RestRequest("audio/path");
            request.AddParameter("dam_id", GetDamId(chapter));
            request.AddParameter("book_id", chapter.Book.DbpId);
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
            var restClient = new RestClient("http://dbt.io/");
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
            return GetCollectionId(chapter.Book.Group);
        }

        private char GetCollectionId(BookGroup group)
        {
            if (group.Parent == BookGroup.EntireBible)
            {
                if (group == BookGroup.OldTestament)
                {
                    return 'O';
                }
                if (group == BookGroup.NewTestament)
                {
                    return 'N';
                }
                throw new Exception(string.Concat("Unexpected book group: ", group.Name));
            }

            return GetCollectionId(group.Parent);
        }
    }
}
