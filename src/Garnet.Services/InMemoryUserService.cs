using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using System.Collections.Concurrent;

namespace Garnet.Services
{
    public class InMemoryUserService : IUserService
    {
        private static readonly ConcurrentDictionary<string, User> _userStore;
        private readonly IBibleMetadataService _bibleMetadataService;

        static InMemoryUserService()
        {
            _userStore = new ConcurrentDictionary<string, User>();
        }

        public InMemoryUserService(IBibleMetadataService bibleMetadataService)
        {
            _bibleMetadataService = bibleMetadataService;
        }

        public User Get(string id)
        {
            User user;
            _userStore.TryGetValue(id, out user);
            return user;
        }

        public User GetOrCreate(string id)
        {
            return _userStore.GetOrAdd(id, x => new User
            {
                Id = id,
                CurrentChapter = _bibleMetadataService.GetDefaultChapter()
            });
        }

        public User AddOrUpdate(User user)
        {
            return _userStore.AddOrUpdate(user.Id, user, (id, existing) =>
            {
                existing.CurrentChapter = user.CurrentChapter;
                return existing;
            });
        }
    }
}
