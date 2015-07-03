using System;
using System.Threading.Tasks;
using Garnet.Domain.Entities;
using Garnet.Domain.Repositories;
using StackExchange.Redis;
using System.Collections.Generic;

namespace Garnet.DataAccess
{
    public class UserRepository : IUserRepository
    {
        private readonly ConnectionMultiplexer _redis;

        public UserRepository(ConnectionMultiplexer redis)
        {
            _redis = redis;
        }

        public async Task<User> GetAsync(string id)
        {
            var user = new User();
            var userHash = await _redis.GetDatabase().HashGetAllAsync(GetUserIdKey(id));
            MapHashToUser(userHash, user);
            return user;
        }

        private static void MapHashToUser(HashEntry[] userHash, User user)
        {
            foreach (var entry in userHash)
            {
                if (UserPropertySetters.ContainsKey(entry.Name))
                {
                    UserPropertySetters[entry.Name](user, entry.Value);
                }
            }
        }

        private static readonly IDictionary<string, Action<User, RedisValue>> UserPropertySetters = new Dictionary<string, Action<User, RedisValue>>
        {
            { "phoneNumber", (user, value) => user.PhoneNumber = value },
            { "chapterNumber", (user, value) => user.ChapterNumber = (int)value },
            { "audioVolumeCode", (user, value) => user.AudioVolumeCode = value },
            { "isDramaticAudioSelected", (user, value) => user.IsDramaticAudioSelected = (bool)value }
        };

        public async Task<User> AddOrUpdateAsync(User user)
        {
            var db = _redis.GetDatabase();
            await db.HashSetAsync(GetUserIdKey(user.Id), new[]
            {
                new HashEntry("phoneNumber", user.PhoneNumber),
                new HashEntry("chapterNumber", user.ChapterNumber),
                new HashEntry("audioVolumeCode", user.AudioVolumeCode),
                new HashEntry("isDramaticAudioSelected", user.IsDramaticAudioSelected)
            });
            await db.StringSetAsync(GetPhoneNumberKey(user.PhoneNumber), user.Id);
            return user;
        }

        public async Task<User> GetByPhoneNumberAsync(string phoneNumber)
        {
            var db = _redis.GetDatabase();
            var userId = await db.StringGetAsync(GetPhoneNumberKey(phoneNumber));
            if (userId.IsNull)
            {
                return null;
            }
            return await GetAsync(userId);
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
