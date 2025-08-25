using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public sealed class SchedulerService : ISchedulerService
    {
        private readonly IBackupService _backup;
        private readonly IProvenanceLogger _log;
        private readonly IPnccLChecker _pncc;
        private readonly string _file;
        private Timer? _timer;
        public SchedulerService(IBackupService backup, IProvenanceLogger log, IPnccLChecker pncc)
        {
            _backup = backup; _log = log; _pncc = pncc;
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(dataDir);
            _file = Path.Combine(dataDir, "scheduler.json");
        }
        public void Start() => _timer = new Timer(async _ => await Tick(), null, 60000, 60000);
        public void Stop() { _timer?.Dispose(); _timer = null; }
        public void Dispose() => Stop();

        private async Task<List<ScheduledTask>> LoadAsync()
        {
            if (!File.Exists(_file)) return new();
            var json = await File.ReadAllTextAsync(_file);
            return string.IsNullOrWhiteSpace(json) ? new() : (JsonSerializer.Deserialize<List<ScheduledTask>>(json) ?? new());
        }
        private Task SaveAsync(List<ScheduledTask> tasks)
            => File.WriteAllTextAsync(_file, JsonSerializer.Serialize(tasks, new JsonSerializerOptions{WriteIndented=true}));

        public async Task<IReadOnlyList<ScheduledTask>> GetTasksAsync() => await LoadAsync();
        public async Task AddTaskAsync(ScheduledTask task)
        {
            var list = await LoadAsync();
            list.Add(task);
            await SaveAsync(list);
        }
        public async Task RemoveTaskAsync(Guid id)
        {
            var list = await LoadAsync();
            list.RemoveAll(t => t.Id == id);
            await SaveAsync(list);
        }
        private async Task Tick()
        {
            var now = DateTime.Now; // local
            var list = await LoadAsync();
            foreach (var t in list)
            {
                if (!t.IsEnabled) continue;
                if (CronHelper.IsMatch(now, t.CronSchedule))
                {
                    if (!await _pncc.ValidateAsync("schedule.execute", t.TaskName)) continue;
                    await _log.LogAsync("schedule.execute", t.TaskName);
                    try { await _backup.CreateBackupAsync(new ServerInstance { Id = t.TargetServerId, InstallPath = Path.Combine("C:\\", "PulsePanel", "Servers", t.TargetServerId.ToString()) }); }
                    catch { }
                }
            }
        }
    }
}
