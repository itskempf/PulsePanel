using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PulsePanel.App.Models;

namespace PulsePanel.App.Services
{
    public interface IExecutionManager
    {
        ReadOnlyObservableCollection<ExecutionJob> Jobs { get; }
        int MaxParallelism { get; set; }

        ExecutionJob Enqueue(Blueprint bp, ExecutionActionType action, ExecutionOptions options);
        Task CancelAsync(Guid jobId);
    }
}