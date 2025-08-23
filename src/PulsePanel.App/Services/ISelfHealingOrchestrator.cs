using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface ISelfHealingOrchestrator
    {
        bool Enabled { get; set; }
        Task ScanAndHealAsync(Blueprint bp, string? nodeId, CancellationToken ct);
        void StartScheduler(TimeSpan interval);
        void StopScheduler();
    }
}