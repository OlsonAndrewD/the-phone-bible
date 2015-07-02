using Garnet.Domain.Services;
using Microsoft.AspNet.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Garnet.Api.Controllers
{
    [Route("audio")]
    public class AudioController : Controller
    {
        private readonly IShortUrlService _shortUrlService;

        public AudioController(IShortUrlService shortUrlService)
        {
            _shortUrlService = shortUrlService;
        }

        [Route("{shortCode}")]
        public async Task<IActionResult> GetAudio(string shortCode)
        {
            var longUrl = await _shortUrlService.GetLongUrlAsync(shortCode);
            if (longUrl == null)
            {
                return new HttpNotFoundResult();
            }
            return new RedirectResult(longUrl);
        }
    }
}
