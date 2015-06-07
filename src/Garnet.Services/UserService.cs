using System;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;
using System.Collections.Concurrent;

namespace Garnet.Services
{
    public class UserService : IUserService
    {
        private static readonly ConcurrentDictionary<string, User> _userStore;
        private readonly IContentService _contentService;

        static UserService()
        {
            _userStore = new ConcurrentDictionary<string, User>();
        }

        public UserService(IContentService contentService)
        {
            _contentService = contentService;
        }

        public User GetOrCreate(string id)
        {
            return _userStore.GetOrAdd(id, x => new User
            {
                Id = id,
                CurrentContentSectionId = _contentService.GetDefaultSectionId()
            });
        }

        public User AddOrUpdate(User user)
        {
            return _userStore.AddOrUpdate(user.Id, user, (id, existing) =>
            {
                existing.CurrentContentSectionId = user.CurrentContentSectionId;
                return existing;
            });
        }
    }
}
