
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;

namespace PulsePanel.ViewModels
{
    public sealed class SchedulerViewModel : ObservableObject
    {
        private readonly ISchedulerService _sched;
        public ObservableCollection<ScheduledTask> Tasks { get; } = new();
        public AsyncCommand Refresh { get; }
        public AsyncCommand Add { get; }
        public AsyncCommand<ScheduledTask> Remove { get; }

        private string _name = "Backup"; public string Name { get => _name; set => SetProperty(ref _name, value); }
        private string _cron = "0 * * * *"; public string Cron { get => _cron; set => SetProperty(ref _cron, value); }
        private string _target = string.Empty; public string Target { get => _target; set => SetProperty(ref _target, value); }

        public SchedulerViewModel(ISchedulerService sched)
        {
            _sched = sched; Refresh = new AsyncCommand(LoadAsync); Add = new AsyncCommand(AddAsync); Remove = new AsyncCommand<ScheduledTask>(RemoveAsync);
        }
        public async Task LoadAsync()
        {
            var list = await _sched.GetTasksAsync();
            Tasks.Clear(); foreach (var t in list.OrderBy(t => t.TaskName)) Tasks.Add(t);
        }
        private async Task AddAsync()
        {
            Guid serverId = Guid.TryParse(Target, out var gid) ? gid : Guid.Empty;
            await _sched.AddTaskAsync(new ScheduledTask{ Id = Guid.NewGuid(), TaskName = Name, CronSchedule = Cron, TargetServerId = serverId, IsEnabled = true });
            await LoadAsync();
        }
        private async Task RemoveAsync(ScheduledTask? t)
        {
            if (t == null) return; await _sched.RemoveTaskAsync(t.Id); await LoadAsync();
        }
    }
}
