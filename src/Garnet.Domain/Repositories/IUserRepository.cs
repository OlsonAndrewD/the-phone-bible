using System.Threading.Tasks;

namespace Garnet.Domain.Repositories
{
    public interface IUserRepository
    {
        Task SetUserPhoneNumber(string userId, string phoneNumber);
        Task<string> GetUserIdByPhoneNumberAsync(string phoneNumber);
        Task<int?> GetCurrentChapterNumber(string userId);
        Task SetCurrentChapterNumber(string userId, int chapterNumber);
    }
}
