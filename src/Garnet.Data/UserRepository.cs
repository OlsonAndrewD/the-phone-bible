using System.Threading.Tasks;
using Garnet.Domain.Repositories;
using StackExchange.Redis;

namespace Garnet.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly ConnectionMultiplexer _redis;

        public UserRepository(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<int?> GetCurrentChapterNumber(string userId)
        {
            var result = await _redis.GetDatabase().StringGetAsync(GetUserIdKey(userId));
            if (!result.IsNull)
            {
                int chapterNumber;
                if (result.TryParse(out chapterNumber))
                {
                    return chapterNumber;
                }
            }
            return null;
        }

        public async Task<string> GetUserIdByPhoneNumberAsync(string phoneNumber)
        {
            return await _redis.GetDatabase().StringGetAsync(GetPhoneNumberKey(phoneNumber));
        }

        public async Task SetCurrentChapterNumber(string userId, int chapterNumber)
        {
            await _redis.GetDatabase().StringSetAsync(GetUserIdKey(userId), chapterNumber);
        }

        public async Task SetUserPhoneNumber(string userId, string phoneNumber)
        {
            await _redis.GetDatabase().StringSetAsync(GetPhoneNumberKey(phoneNumber), userId);
        }

        private static string GetUserIdKey(string userId)
        {
            return string.Concat("userId:", userId);
        }

        private static string GetPhoneNumberKey(string phoneNumber)
        {
            return string.Concat("phone:", phoneNumber);
        }
    }
}
