using System;
using System.Threading.Tasks;
using Garnet.Domain.Entities;
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

        public async Task<string> GetUserIdByPhoneNumberAsync(string phoneNumber)
        {
            var phoneNumberKey = string.Concat("phone:", phoneNumber);
            return await _redis.GetDatabase().StringGetAsync(phoneNumberKey);

            //User user = null;
            //if (!userId.IsNull)
            //{
            //    user = new User { Id = userId };

            //    var userKey = string.Concat("userId:", userId);
            //    var chapterString = await db.StringGetAsync(userKey);
            //    if (!chapterString.IsNull)
            //    {
            //        user.CurrentChapter = Chapter.Parse(chapterString);
            //    }
            //}

            //return user;
        }
    }
}
