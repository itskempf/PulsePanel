using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PulsePanel.App.Services
{
    public interface IActionHandler
    {
        Task ExecuteAsync(Dictionary<string, string> parameters, CancellationToken ct = default);
    }
}