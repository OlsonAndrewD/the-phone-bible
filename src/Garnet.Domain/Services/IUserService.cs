using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IUserService
    {
        Task<User> GetAsync(string id);
        Task<User> GetOrCreateAsync(string id);
        Task<User> AddOrUpdateAsync(User user);
    }
}
