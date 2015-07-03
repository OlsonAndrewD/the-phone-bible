using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetAsync(string id);
        Task<User> GetByPhoneNumberAsync(string phoneNumber);
        Task<User> AddOrUpdateAsync(User user);
    }
}
