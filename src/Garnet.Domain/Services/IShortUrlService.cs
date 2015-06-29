using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IShortUrlService
    {
        Task<string> GetOrCreateShortCodeAsync(string longUrl);
        Task<string> GetLongUrlAsync(string shortCode);
    }
}
