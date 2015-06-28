using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        Task<string> GetContentUrlAsync(User user);
        Task<string> GetCopyrightInfoAsync(User user);
    }
}
