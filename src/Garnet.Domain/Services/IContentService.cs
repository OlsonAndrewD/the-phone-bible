using Garnet.Domain.Entities;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        Task<string> GetContentUrlAsync(Chapter chapter);
        Task<string> GetCopyrightInfoAsync(Chapter chapter);
    }
}
