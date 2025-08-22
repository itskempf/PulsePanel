using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public interface IServerService
{
    event EventHandler? ServersChanged;

    Task<IReadOnlyList<ServerEntry>> GetServersAsync(CancellationToken ct = default);

    Task<bool> StartAsync(string serverId, CancellationToken ct = default);
    Task<bool> StopAsync(string serverId, CancellationToken ct = default);
}
