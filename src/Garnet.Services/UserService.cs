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
        private const string DefaultAudioVolumeCode = "ENGKJV";

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
                }
                await _userRepository.AddOrUpdateAsync(user);
            }

            return user;
        }

        public async Task<User> GetByPhoneNumberAsync(string phoneNumber)
        {
            return await _userRepository.GetByPhoneNumberAsync(phoneNumber);
        }

        public async Task<User> GetByPhoneNumberOrCreateAsync(string phoneNumber)
        {
            var user = await GetByPhoneNumberAsync(phoneNumber);
            if (user == null)
            {
                user = await AddOrUpdateAsync(new User
                {
                    PhoneNumber = phoneNumber,
                    ChapterNumber = 1,
                    AudioVolumeCode = DefaultAudioVolumeCode
                });
            }
            return user;
        }
    }
}
