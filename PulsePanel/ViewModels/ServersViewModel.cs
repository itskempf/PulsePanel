using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;

namespace PulsePanel.ViewModels
{
    public sealed class ServersViewModel : ObservableObject
    {
        private readonly IServerService _servers;
        private readonly IServerProcessService _proc;
        public ObservableCollection<ServerInstance> Items { get; } = new();
        public ObservableCollection<string> ConsoleLines { get; } = new();
        private ServerInstance? _selected;
        public ServerInstance? Selected { get => _selected; set { SetProperty(ref _selected, value); ConsoleLines.Clear(); } }

        public AsyncCommand Refresh { get; }
        public AsyncCommand<ServerInstance> Start { get; }
        public AsyncCommand<ServerInstance> Stop { get; }

        public ServersViewModel(IServerService servers, IServerProcessService proc)
        {
            _servers = servers; _proc = proc;
            Refresh = new AsyncCommand(LoadAsync);
            Start = new AsyncCommand<ServerInstance>(StartAsync, s => s != null && s.Status != ServerStatus.Running);
            Stop = new AsyncCommand<ServerInstance>(StopAsync,  s => s != null && s.Status == ServerStatus.Running);
            _proc.OutputReceived += Proc_OutputReceived;
        }

        private void Proc_OutputReceived(object? sender, ServerOutputEventArgs e)
        {
            if (Selected != null && e.ServerId == Selected.Id)
            {
                App.Current.Dispatcher.Invoke(() => ConsoleLines.Add(e.Line));
            }
        }

        public async Task LoadAsync()
        {
            var list = await _servers.GetAllServersAsync();
            Items.Clear(); foreach (var s in list.OrderBy(s => s.InstanceName)) Items.Add(s);
        }
        private async Task StartAsync(ServerInstance? s)
        {
            if (s == null) return; await _proc.StartServerAsync(s); await _servers.UpdateAsync(s); await LoadAsync();
        }
        private async Task StopAsync(ServerInstance? s)
        {
            if (s == null) return; await _proc.StopServerAsync(s); await _servers.UpdateAsync(s); await LoadAsync();
        }
    }
}
