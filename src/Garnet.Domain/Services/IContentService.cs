using Garnet.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface IContentService
    {
        Task<IEnumerable<AudioVolume>> GetAvailableVolumesAsync();
        Task<string> GetContentUrlAsync(User user);
        Task<string> GetCopyrightInfoAsync(User user);
    }
}
