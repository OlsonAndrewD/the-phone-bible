using Garnet.Api.ActionResults;
using Garnet.Api.Routes;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using Microsoft.AspNet.Http;
using Microsoft.AspNet.Mvc;
using System;
using System.Threading.Tasks;

namespace Garnet.Api.Controllers
{
    [Route(TwilioSmsRoutes.Root)]
    public class TwilioSmsController : Controller
    {
        private readonly IUserService _userService;
        private readonly IBibleMetadataService _bibleMetadataService;
        private readonly IContentService _contentService;
        private readonly IShortUrlService _shortUrlService;

        public TwilioSmsController(IUserService userService, IBibleMetadataService bibleMetadataService, IContentService contentService, IShortUrlService shortUrlService)
        {
            _userService = userService;
            _bibleMetadataService = bibleMetadataService;
            _contentService = contentService;
            _shortUrlService = shortUrlService;
        }

        [Route("response")]
        [HttpGet]
        public async Task<IActionResult> ReceiveSms(
            [FromQuery(Name = "From")] string fromPhoneNumber,
            [FromQuery(Name = "Body")] string messageBody)
        {
            if (messageBody == "NEXT")
            {
                return await GetNext(fromPhoneNumber);
            }

            return new TwilioResponseResult(x => x.Message("Send NEXT to get next chapter."));
        }

        public async Task<TwilioResponseResult> GetNext(string fromPhoneNumber)
        {
            var user = await _userService.GetAsync(fromPhoneNumber);
            if (user == null)
            {
                user = await _userService.GetOrCreateAsync(fromPhoneNumber);
            }
            else
            {
                user.CurrentChapterNumber++;
            }

            var nextChapter = _bibleMetadataService.GetChapterByNumber(user.CurrentChapterNumber);
            if (nextChapter == null)
            {
                user.CurrentChapterNumber = 1;
            }

            // Fire and forget
            _userService.AddOrUpdateAsync(user);

            var getContentUrlTask = _contentService.GetContentUrlAsync(user);
            var getCopyrightInfoTask = _contentService.GetCopyrightInfoAsync(user);

            var contentUrl = await getContentUrlTask;
            var copyrightInfo = await getCopyrightInfoTask;

            var shortCode = await _shortUrlService.GetOrCreateShortCodeAsync(contentUrl);

            var responseMessage = string.Format("{0}: {1} {2}",
                string.Join(" ", nextChapter.Book.Name, nextChapter.ChapterNumber),
                CreateShortUrl(nextChapter, shortCode),
                copyrightInfo);

            return new TwilioResponseResult(x => x.Message(responseMessage));
        }

        private string CreateShortUrl(Chapter chapter, string shortCode)
        {
            var uriBuilder = new UriBuilder(string.Join("://", Request.Scheme, Request.Host.Value));
            uriBuilder.Path = string.Join("/",
                AudioRoutes.Root,
                chapter.Book.DbpId,
                chapter.ChapterNumber,
                shortCode);
            return uriBuilder.Uri.ToString();
        }
    }
}
