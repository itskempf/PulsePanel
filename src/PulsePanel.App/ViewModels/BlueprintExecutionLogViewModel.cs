using CommunityToolkit.Mvvm.ComponentModel;
using PulsePanel.App.Services;
using System.Collections.ObjectModel;

namespace PulsePanel.App.ViewModels
{
    public partial class BlueprintExecutionLogViewModel : ObservableObject
    {
        private readonly IProvenanceLogService _logService;

        [ObservableProperty]
        private ObservableCollection<string> logEntries = new();

        public BlueprintExecutionLogViewModel(IProvenanceLogService logService)
        {
            _logService = logService;
            _logService.LogEntryAdded += OnLogEntryAdded;
        }

        private void OnLogEntryAdded(object? sender, string e)
        {
            App.MainThreadDispatcher(() =>
            {
                LogEntries.Add(e);
            });
        }
    }
}