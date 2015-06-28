using System.Threading.Tasks;

namespace Garnet.Domain.Repositories
{
    public interface IUserRepository
    {
        Task<string> GetUserIdByPhoneNumberAsync(string phoneNumber);
    }
}
