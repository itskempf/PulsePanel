namespace PulsePanel.Core.Services;

public interface IHealthService
{
    // Placeholder for async health computations per server
    Task<string> GetHealthScoreAsync(string serverId, CancellationToken ct = default);
}
