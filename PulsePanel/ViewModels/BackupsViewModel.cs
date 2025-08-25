
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;

namespace PulsePanel.ViewModels
{
    public sealed class BackupsViewModel : ObservableObject
    {
        private readonly IBackupService _backup;
        private readonly IServerService _servers;
        public ObservableCollection<BackupRecord> Items { get; } = new();
        public AsyncCommand Refresh { get; }
        public AsyncCommand Create { get; }
        private string _target = string.Empty; public string Target { get => _target; set => SetProperty(ref _target, value); }
        public BackupsViewModel(IBackupService backup, IServerService servers)
        {
            _backup = backup; _servers = servers; Refresh = new AsyncCommand(LoadAsync); Create = new AsyncCommand(CreateAsync);
        }
        public async Task LoadAsync()
        {
            var list = await _backup.GetBackupsAsync(); Items.Clear(); foreach (var r in list.OrderByDescending(r => r.Timestamp)) Items.Add(r);
        }
        private async Task CreateAsync()
        {
            if (!System.Guid.TryParse(Target, out var gid)) return;
            var s = await _servers.FindAsync(gid); if (s == null) return;
            await _backup.CreateBackupAsync(s); await LoadAsync();
        }
    }
}
