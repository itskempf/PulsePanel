
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public interface ISchedulerService : IDisposable
    {
        Task<IReadOnlyList<ScheduledTask>> GetTasksAsync();
        Task AddTaskAsync(ScheduledTask task);
        Task RemoveTaskAsync(Guid id);
        void Start();
        void Stop();
    }
}
