using Garnet.Domain.Entities;

namespace Garnet.Domain.Services
{
    public interface IUserService
    {
        User Get(string id);
        User GetOrCreate(string id);
        User AddOrUpdate(User user);
    }
}
