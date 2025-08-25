
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IPnccLChecker
    {
        Task<bool> ValidateAsync(string action, string payload);
    }
}
