using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<User> GetByPhoneNumberAsync(string phoneNumber);
    }
}
