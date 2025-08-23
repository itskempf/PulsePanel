using Microsoft.UI.Dispatching;
using PulsePanel.App.State;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PulsePanel.App.ViewModels
{
    public partial class ServersViewModel : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private readonly AppState _state;
        private readonly IServerService _servers;
        private readonly ProvenanceLogger _logger;
        private readonly IHealthMonitorService _health;
        private readonly DispatcherQueue _dispatcher;

        public ReadOnlyObservableCollection<ServerEntry> CoreServers => _state.Servers;

        public ObservableCollection<ServerItem> Servers { get; } = new();

        private ServerItem? _selected;
        public ServerItem? Selected
        {
            get => _selected;
            set
            {
                if (!EqualityComparer<ServerItem?>.Default.Equals(_selected, value))
                {
                    _selected = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(SelectedServerHistory));
                    StartCommand?.NotifyCanExecuteChanged();
                    StopCommand?.NotifyCanExecuteChanged();
                }
            }
        }

        public System.Collections.ObjectModel.ObservableCollection<Models.HealthSnapshot>? SelectedServerHistory => Selected?.History;

        public PulsePanel.App.RelayCommand? StartCommand { get; }
        public PulsePanel.App.RelayCommand? StopCommand { get; }
        public PulsePanel.App.RelayCommand? RefreshCommand { get; }

        public ServersViewModel(AppState state, IServerService serverService, ProvenanceLogger logger, IHealthMonitorService health)
        {
            _state = state;
            _servers = serverService;
            _logger = logger;
            _health = health;
            _dispatcher = DispatcherQueue.GetForCurrentThread();

            // Seed initial projection from AppState
            ProjectFromState();
            _state.PropertyChanged += (_, e) =>
            {
                if (e.PropertyName == nameof(AppState.SelectedServer))
                {
                    var match = Servers.FirstOrDefault(x => x.Id == _state.SelectedServer?.Id);
                    if (match != null) Selected = match;
                }
            };
            // When server list changes, AppState raises via event it subscribes to ServerService; refresh projection
            _servers.ServersChanged += (_, __) => ProjectFromState();

            RefreshCommand = new PulsePanel.App.RelayCommand(async _ =>
            {
                var list = await _servers.GetServersAsync();
                _dispatcher.TryEnqueue(() =>
                {
                    var prevSelId = Selected?.Id;
                    Servers.Clear();
                    foreach (var s in list)
                    {
                        Servers.Add(new ServerItem { Id = s.Id, Name = s.Name, Status = s.Status, Game = s.Game });
                    }
                    if (prevSelId != null)
                    {
                        var m = Servers.FirstOrDefault(x => x.Id == prevSelId);
                        if (m != null) Selected = m; else if (Servers.Count > 0) Selected = Servers[0];
                    }
                    else if (Servers.Count > 0)
                    {
                        Selected = Servers[0];
                    }
                });
            }, _ => true);

            StartCommand = new PulsePanel.App.RelayCommand(async _ =>
            {
                if (Selected == null) return;
                await _servers.StartAsync(Selected.Id);
                _logger.Log(new PulsePanel.Blueprints.Provenance.LogEntry
                {
                    Action = "ui_start",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { { "serverId", Selected.Id } }
                });
            }, _ => Selected != null && !string.Equals(Selected.Status, "running", StringComparison.OrdinalIgnoreCase));

            StopCommand = new PulsePanel.App.RelayCommand(async _ =>
            {
                if (Selected == null) return;
                await _servers.StopAsync(Selected.Id);
                _logger.Log(new PulsePanel.Blueprints.Provenance.LogEntry
                {
                    Action = "ui_stop",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { { "serverId", Selected.Id } }
                });
            }, _ => Selected != null && string.Equals(Selected.Status, "running", StringComparison.OrdinalIgnoreCase));

            // Subscribe to health updates
            _health.HealthUpdated += OnHealthUpdated;
        }

        private void ProjectFromState()
        {
            var dq = DispatcherQueue.GetForCurrentThread();
            void DoProject()
            {
                var prevSelId = Selected?.Id;
                Servers.Clear();
                foreach (var s in _state.Servers)
                {
                    Servers.Add(new ServerItem
                    {
                        Id = s.Id,
                        Name = s.Name,
                        Status = s.Status,
                        Game = s.Game
                    });
                }
                if (prevSelId != null)
                {
                    var m = Servers.FirstOrDefault(x => x.Id == prevSelId);
                    if (m != null) Selected = m; else if (Servers.Count > 0) Selected = Servers[0];
                }
                else if (Servers.Count > 0)
                {
                    Selected = Servers[0];
                }
            }

            if (dq != null)
            {
                dq.TryEnqueue(DoProject);
            }
            else
            {
                DoProject();
            }
        }

        private void OnHealthUpdated(object? sender, ServerHealthSnapshot e)
        {
            _dispatcher?.TryEnqueue(() =>
            {
                var match = Servers.FirstOrDefault(s => s.Id == e.ServerId);
                if (match != null)
                {
                    match.CpuPercent = e.CpuPercent;
                    match.MemoryMb = e.MemoryMb;
                    match.Uptime = e.Uptime;
                    // append history (rolling window 100)
                    var snap = new Models.HealthSnapshot(e.Timestamp, e.CpuPercent, e.MemoryMb, e.Uptime);
                    match.History.Add(snap);
                    if (match.History.Count > 100)
                    {
                        match.History.RemoveAt(0);
                    }
                    if (ReferenceEquals(match, Selected))
                    {
                        OnPropertyChanged(nameof(SelectedServerHistory));
                    }
                }
            });
        }
    }

    public sealed class ServerItem : CommunityToolkit.Mvvm.ComponentModel.ObservableObject
    {
        private string _id = string.Empty;
        private string _name = string.Empty;
        private string _status = string.Empty;
        private string _game = string.Empty;
        private double _cpu;
        private double _mem;
        private TimeSpan _uptime;

        public ServerItem()
        {
            History = new System.Collections.ObjectModel.ObservableCollection<Models.HealthSnapshot>();
        }

        public string Id { get => _id; set => SetProperty(ref _id, value); }
        public string Name { get => _name; set => SetProperty(ref _name, value); }
        public string Status { get => _status; set => SetProperty(ref _status, value); }
        public string Game { get => _game; set => SetProperty(ref _game, value); }
        public double CpuPercent { get => _cpu; set => SetProperty(ref _cpu, value); }
        public double MemoryMb { get => _mem; set => SetProperty(ref _mem, value); }
        public TimeSpan Uptime { get => _uptime; set => SetProperty(ref _uptime, value); }

        public System.Collections.ObjectModel.ObservableCollection<Models.HealthSnapshot> History { get; }
    }
}
