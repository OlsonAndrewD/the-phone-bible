using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IUserService
    {
        Task<User> GetByPhoneNumberAsync(string phoneNumber);
        Task<User> GetByPhoneNumberOrCreateAsync(string phoneNumber);
        Task<User> AddOrUpdateAsync(User user);
    }
}
