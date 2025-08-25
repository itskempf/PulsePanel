
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public sealed class PnccLChecker : IPnccLChecker
    {
        public Task<bool> ValidateAsync(string action, string payload) => Task.FromResult(true);
    }
}
