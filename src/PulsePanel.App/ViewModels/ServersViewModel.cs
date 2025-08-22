using CommunityToolkit.Mvvm.ComponentModel;
using PulsePanel.App.State;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Windows.Input;

namespace PulsePanel.App.ViewModels
{
    public partial class ServersViewModel : ObservableObject
    {
        private readonly AppState _state;
        private readonly IServerService _servers;
        private readonly ProvenanceLogger _logger;

        public ReadOnlyObservableCollection<ServerEntry> Servers => _state.Servers;

        private ServerEntry? _selectedServer;
        public ServerEntry? SelectedServer
        {
            get => _selectedServer;
            set
            {
                if (!EqualityComparer<ServerEntry?>.Default.Equals(_selectedServer, value))
                {
                    _selectedServer = value;
                    OnPropertyChanged();
                    _state.SelectedServer = value;
                    StartCommand?.NotifyCanExecuteChanged();
                    StopCommand?.NotifyCanExecuteChanged();
                }
            }
        }

    public PulsePanel.App.RelayCommand? StartCommand { get; }
    public PulsePanel.App.RelayCommand? StopCommand { get; }

        public ServersViewModel(AppState state, IServerService serverService, ProvenanceLogger logger)
        {
            _state = state;
            _servers = serverService;
            _logger = logger;
            SelectedServer = _state.SelectedServer;

            StartCommand = new PulsePanel.App.RelayCommand(async _ =>
            {
                if (SelectedServer == null) return;
                await _servers.StartAsync(SelectedServer.Id);
                _logger.Log(new PulsePanel.Blueprints.Provenance.LogEntry
                {
                    Action = "ui_start",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { { "serverId", SelectedServer.Id } }
                });
            }, _ => SelectedServer != null && !string.Equals(SelectedServer.Status, "running", StringComparison.OrdinalIgnoreCase));

            StopCommand = new PulsePanel.App.RelayCommand(async _ =>
            {
                if (SelectedServer == null) return;
                await _servers.StopAsync(SelectedServer.Id);
                _logger.Log(new PulsePanel.Blueprints.Provenance.LogEntry
                {
                    Action = "ui_stop",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { { "serverId", SelectedServer.Id } }
                });
            }, _ => SelectedServer != null && string.Equals(SelectedServer.Status, "running", StringComparison.OrdinalIgnoreCase));
        }
    }
}
