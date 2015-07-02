using Garnet.Domain.Repositories;
using StackExchange.Redis;
using System.Threading.Tasks;
using System;

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
            var db = _redis.GetDatabase();
            var longUrlKey = GetLongUrlKey(longUrl);
            var shortCodeKey = GetShortCodeKey(shortCode);

            var setKeyTasks = new[] {
                db.StringSetAsync(longUrlKey, shortCode),
                db.StringSetAsync(shortCodeKey, longUrl)
            };

            foreach(var task in setKeyTasks)
            {
                await task;
            }

            // Don't wait for these.
            db.KeyExpireAsync(longUrlKey, TimeSpan.FromDays(2));
            db.KeyExpireAsync(shortCodeKey, TimeSpan.FromDays(2));
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
