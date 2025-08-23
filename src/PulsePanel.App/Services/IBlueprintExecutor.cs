using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IBlueprintExecutor
    {
        Task ExecuteInstallAsync(Blueprint bp, CancellationToken ct = default);
        Task ExecuteUpdateAsync(Blueprint bp, CancellationToken ct = default);
        Task ExecuteValidateAsync(Blueprint bp, CancellationToken ct = default);
    }
}