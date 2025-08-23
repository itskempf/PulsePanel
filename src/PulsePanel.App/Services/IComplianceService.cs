using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IComplianceService
    {
        Task<ComplianceReport> ScanAsync(Blueprint blueprint, string? nodeId, CancellationToken ct);
    }
}