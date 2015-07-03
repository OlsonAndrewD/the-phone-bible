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
            string responseMessage;
            switch ((messageBody ?? string.Empty).Trim().ToLower())
            {
                case "next":
                    responseMessage = await GetNext(fromPhoneNumber);
                    break;
                case "prev":
                    responseMessage = await GetPrevious(fromPhoneNumber);
                    break;
                default:
                    responseMessage = string.Join(Environment.NewLine,
                        await GetCurrent(fromPhoneNumber),
                        string.Empty,
                        "Send \"next\" for next chapter.",
                        "Send \"prev\" for previous chapter."
                        );
                    break;
            }
            return new TwilioResponseResult(x => x.Message(responseMessage));
        }

        private async Task<string> GetCurrent(string fromPhoneNumber)
        {
            return await UpdateUserAndGetResponseMessage(fromPhoneNumber, x => x.ChapterNumber);
        }

        private async Task<string> GetPrevious(string fromPhoneNumber)
        {
            return await UpdateUserAndGetResponseMessage(fromPhoneNumber, x => x.ChapterNumber - 1);
        }

        private async Task<string> GetNext(string fromPhoneNumber)
        {
            return await UpdateUserAndGetResponseMessage(fromPhoneNumber, x => x.ChapterNumber + 1);
        }

        private async Task<string> UpdateUserAndGetResponseMessage(string fromPhoneNumber, Func<User, int> getNewChapterNumber)
        {
            var newChapterNumber = 1;
            var user = await _userService.GetByPhoneNumberAsync(fromPhoneNumber);
            if (user == null)
            {
                user = await _userService.GetByPhoneNumberOrCreateAsync(fromPhoneNumber);
            }
            else
            {
                newChapterNumber = getNewChapterNumber(user);
            }

            var newChapter = _bibleMetadataService.GetChapterByNumber(newChapterNumber);
            if (newChapter == null)
            {
                newChapterNumber = 1;
                newChapter = _bibleMetadataService.GetChapterByNumber(newChapterNumber);
            }

            if (newChapterNumber != user.ChapterNumber)
            {
                user.ChapterNumber = newChapterNumber;

                // Fire and forget
                _userService.AddOrUpdateAsync(user);
            }

            var getContentUrlTask = _contentService.GetContentUrlAsync(user);
            var getCopyrightInfoTask = _contentService.GetCopyrightInfoAsync(user);

            var contentUrl = await getContentUrlTask;
            var copyrightInfo = await getCopyrightInfoTask;

            var shortCode = await _shortUrlService.GetOrCreateShortCodeAsync(contentUrl);

            return string.Format("{0}: {1} {2}",
                newChapter,
                CreateShortUrl(newChapter, shortCode),
                copyrightInfo);
        }

        private string CreateShortUrl(Chapter chapter, string shortCode)
        {
            var uriBuilder = new UriBuilder(string.Join("://", Request.Scheme, Request.Host.Value));
            uriBuilder.Path = string.Join("/",
                AudioRoutes.Root,
                shortCode);
            return uriBuilder.Uri.ToString();
        }
    }
}
