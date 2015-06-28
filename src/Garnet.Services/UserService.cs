using System;
using Garnet.Domain.Entities;
using Garnet.Domain.Services;

namespace Garnet.Services
{
    public class UserService : IUserService
    {
        private readonly IBibleMetadataService _bibleMetadataService;

        public UserService(IBibleMetadataService bibleMetadataService)
        {
            _bibleMetadataService = bibleMetadataService;
        }

        public User AddOrUpdate(User user)
        {
            throw new NotImplementedException();
        }

        public User Get(string id)
        {
            throw new NotImplementedException();
        }

        public User GetOrCreate(string id)
        {
            throw new NotImplementedException();
        }
    }
}
