using System;
using System.Threading.Tasks;

namespace Garnet.Domain.Services
{
    public interface ITimeService
    {
        Task<TimeSpan?> GetLocalTimeAsync(string city, string state, string zip);
    }
}
