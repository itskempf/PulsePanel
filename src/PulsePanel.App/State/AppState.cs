using CommunityToolkit.Mvvm.ComponentModel;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;

namespace PulsePanel.App.State;

public partial class AppState : ObservableObject
{
    private readonly ObservableCollection<ServerEntry> _servers = new();
    public ReadOnlyObservableCollection<ServerEntry> Servers { get; }
    private ServerEntry? _selectedServer;
    public ServerEntry? SelectedServer
    {
        get => _selectedServer;
        set => SetProperty(ref _selectedServer, value);
    }

    public AppState()
    {
        Servers = new ReadOnlyObservableCollection<ServerEntry>(_servers);
    }

    public async Task InitializeAsync(IServerService serverService, CancellationToken ct = default)
    {
        await RefreshAsync(serverService, ct).ConfigureAwait(false);
        serverService.ServersChanged += async (_, __) => await RefreshAsync(serverService).ConfigureAwait(false);
    }

    private async Task RefreshAsync(IServerService serverService, CancellationToken ct = default)
    {
        var list = await serverService.GetServersAsync(ct).ConfigureAwait(false);
        var dq = Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread();
        if (dq is not null)
        {
            dq.TryEnqueue(() =>
            {
                _servers.Clear();
                foreach (var s in list) _servers.Add(s);
                if (SelectedServer is null && _servers.Count > 0) SelectedServer = _servers[0];
            });
        }
        else
        {
            _servers.Clear();
            foreach (var s in list) _servers.Add(s);
            if (SelectedServer is null && _servers.Count > 0) SelectedServer = _servers[0];
        }
    }
}
