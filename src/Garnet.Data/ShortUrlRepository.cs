using Garnet.Domain.Repositories;
using StackExchange.Redis;
using System.Threading.Tasks;

namespace Garnet.DataAccess
{
    public class ShortUrlRepository : IShortUrlRepository
    {
        private readonly ConnectionMultiplexer _redis;

        public ShortUrlRepository(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<string> GetShortCodeAsync(string longUrl)
        {
            return await _redis.GetDatabase().StringGetAsync(GetLongUrlKey(longUrl));
        }

        public async Task<string> GetLongUrlAsync(string shortCode)
        {
            return await _redis.GetDatabase().StringGetAsync(GetShortCodeKey(shortCode));
        }

        public async Task SetShortCodeAsync(string longUrl, string shortCode)
        {
            var tasks = new[] {
                _redis.GetDatabase().StringSetAsync(GetLongUrlKey(longUrl), shortCode),
                _redis.GetDatabase().StringSetAsync(GetShortCodeKey(shortCode), longUrl)
            };

            foreach(var task in tasks)
            {
                await task;
            }
        }

        private string GetShortCodeKey(string url)
        {
            return string.Concat("shortCode:", url);
        }

        private string GetLongUrlKey(string url)
        {
            return string.Concat("longUrl:", url);
        }
    }
}
