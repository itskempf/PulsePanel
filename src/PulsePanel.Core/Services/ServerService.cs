using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class ServerService : IServerService
{
    private readonly ServerStore _store;
    private readonly ServerProcessService _process;

    public event EventHandler? ServersChanged;

    public ServerService(ServerStore store, ServerProcessService process)
    {
        _store = store;
        _process = process;
    }

    public Task<IReadOnlyList<ServerEntry>> GetServersAsync(CancellationToken ct = default)
    {
        var list = _store.Load();
        // Enrich status from process map
        foreach (var s in list)
        {
            var st = _process.GetServerStatus(s);
            s.Status = st.ToString().ToLowerInvariant();
        }
        return Task.FromResult<IReadOnlyList<ServerEntry>>(list);
    }

    public Task<bool> StartAsync(string serverId, CancellationToken ct = default)
    {
    var list = _store.Load();
    var s = list.FirstOrDefault(x => x.Id == serverId);
    if (s == null) return Task.FromResult(false);
    // simulate start
    _process.Start(serverId);
        ServersChanged?.Invoke(this, EventArgs.Empty);
        return Task.FromResult(true);
    }

    public Task<bool> StopAsync(string serverId, CancellationToken ct = default)
    {
        var list = _store.Load();
        var s = list.FirstOrDefault(x => x.Id == serverId);
        if (s == null) return Task.FromResult(false);
    _process.Stop(serverId);
        ServersChanged?.Invoke(this, EventArgs.Empty);
        return Task.FromResult(true);
    }
}
