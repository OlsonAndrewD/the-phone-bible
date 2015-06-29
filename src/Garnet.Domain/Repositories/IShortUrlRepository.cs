using System.Threading.Tasks;

namespace Garnet.Domain.Repositories
{
    public interface IShortUrlRepository
    {
        Task<string> GetShortCodeAsync(string longUrl);
        Task SetShortCodeAsync(string longUrl, string shortCode);
        Task<string> GetLongUrlAsync(string shortCode);
    }
}
