using System;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using Garnet.Domain.Repositories;
using System.Threading.Tasks;

namespace Garnet.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;

        public UserService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<User> AddOrUpdateAsync(User user)
        {
            if (user != null)
            {
                if (user.Id == null)
                {
                    user.Id = Guid.NewGuid().ToString();
                    await _userRepository.SetUserPhoneNumber(user.Id, user.PhoneNumber);
                }

                await _userRepository.SetCurrentChapterNumber(user.Id, user.CurrentChapterNumber);
            }

            return user;
        }

        public async Task<User> GetAsync(string phoneNumber)
        {
            var userId = await _userRepository.GetUserIdByPhoneNumberAsync(phoneNumber);
            if (userId == null)
            {
                return null;
            }

            var currentChapterNumber = await _userRepository.GetCurrentChapterNumber(userId);

            return new User
            {
                Id = userId,
                PhoneNumber = phoneNumber,
                CurrentChapterNumber = currentChapterNumber ?? 1
            };
        }

        public async Task<User> GetOrCreateAsync(string phoneNumber)
        {
            var user = await GetAsync(phoneNumber);
            if (user == null)
            {
                user = await AddOrUpdateAsync(new User
                {
                    PhoneNumber = phoneNumber,
                    CurrentChapterNumber = 1
                });
            }
            return user;
        }
    }
}
