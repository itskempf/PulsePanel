using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.ObjectModel;
using PulsePanel.App.Models;
using PulsePanel.App.Services;

namespace PulsePanel.App.ViewModels
{
    public partial class JobQueueViewModel : ObservableObject
    {
        private readonly IExecutionManager _mgr;

        public ReadOnlyObservableCollection<ExecutionJob> Jobs => _mgr.Jobs;

        public JobQueueViewModel(IExecutionManager mgr) { _mgr = mgr; }

        [RelayCommand]
        private async Task CancelAsync(ExecutionJob? job)
        {
            if (job is null) return;
            await _mgr.CancelAsync(job.Id);
        }
    }
}