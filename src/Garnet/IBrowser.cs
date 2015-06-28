using Garnet.Api.ActionResults;
using System.Threading.Tasks;

namespace Garnet.Api
{
    public interface IBrowser
    {
        Task<TwilioResponseResult> HandleBrowseAsync(string phoneNumber, bool navigatingUp);
        Task<TwilioResponseResult> HandleSelection(string phoneNumber, string selection);
    }
}
