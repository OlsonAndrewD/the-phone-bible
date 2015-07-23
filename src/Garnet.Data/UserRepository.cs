using System;
using System.Threading.Tasks;
using Garnet.Domain.Entities;
using Garnet.Domain.Repositories;
using StackExchange.Redis;
using System.Collections.Generic;
using System.Linq;

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
            var userHash = await _redis.GetDatabase().HashGetAllAsync(GetUserIdKey(id));
            return MapHashToUser(userHash);
        }

        public async Task<User> AddOrUpdateAsync(User user)
        {
            var db = _redis.GetDatabase();
            var userIdKey = GetUserIdKey(user.Id);

            await db.HashSetAsync(userIdKey, MapUserToHashEntries(user).ToArray());
            if (user.ReminderTimeInMinutes == null)
            {
                await db.HashDeleteAsync(userIdKey, "reminderTimeInMinutes");
            }
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

        #region Mapping

        private static User MapHashToUser(IEnumerable<HashEntry> userHash)
        {
            var user = new User();
            foreach (var entry in userHash)
            {
                if (UserPropertySetters.ContainsKey(entry.Name))
                {
                    var setUserProperty = UserPropertySetters[entry.Name];
                    setUserProperty(user, entry.Value);
                }
            }
            return user;
        }

        private static readonly IDictionary<string, Action<User, RedisValue>> UserPropertySetters = new Dictionary<string, Action<User, RedisValue>>
        {
            { "phoneNumber", (user, value) => user.PhoneNumber = value },
            { "chapterNumber", (user, value) => user.ChapterNumber = (int)value },
            { "audioVolumeCode", (user, value) => user.AudioVolumeCode = value },
            { "isDramaticAudioSelected", (user, value) => user.IsDramaticAudioSelected = (bool)value },
            { "reminderTimeInMinutes", (user, value) => user.ReminderTimeInMinutes = value.IsNull ? (int?)null : (int)value }
        };

        private static IEnumerable<HashEntry> MapUserToHashEntries(User user)
        {
            var hashEntries = new List<HashEntry>
            {
                new HashEntry("phoneNumber", user.PhoneNumber),
                new HashEntry("chapterNumber", user.ChapterNumber),
                new HashEntry("audioVolumeCode", user.AudioVolumeCode),
                new HashEntry("isDramaticAudioSelected", user.IsDramaticAudioSelected),
            };

            if (user.ReminderTimeInMinutes != null)
            {
                hashEntries.Add(new HashEntry("reminderTimeInMinutes", user.ReminderTimeInMinutes));
            }

            return hashEntries;
        }

        #endregion
    }
}
