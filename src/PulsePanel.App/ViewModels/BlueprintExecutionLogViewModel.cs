using CommunityToolkit.Mvvm.ComponentModel;
using PulsePanel.App.Services;
using System.Collections.ObjectModel;
using System;
using PulsePanel.App.Models;

namespace PulsePanel.App.ViewModels
{
    public partial class BlueprintExecutionLogViewModel : ObservableObject
    {
        private readonly IProvenanceLogService _logService;

        [ObservableProperty] private string filterText = "";
        [ObservableProperty] private bool showInfo = true;
        [ObservableProperty] private bool showWarnings = true;
        [ObservableProperty] private bool showErrors = true;

        public ObservableCollection<LogEntry> AllEntries { get; } = new();
        public ObservableCollection<LogEntry> FilteredEntries { get; } = new();

        public BlueprintExecutionLogViewModel(IProvenanceLogService logService)
        {
            _logService = logService;
            _logService.LogEntryAdded += OnLogEntryAdded;
        }

        private void OnLogEntryAdded(object? sender, LogEntry entry)
        {
            App.MainThreadDispatcher(() =>
            {
                AllEntries.Add(entry);
                if (PassesFilter(entry))
                    FilteredEntries.Add(entry);
            });
        }

        partial void OnFilterTextChanged(string value) => ApplyFilters();
        partial void OnShowInfoChanged(bool value) => ApplyFilters();
        partial void OnShowWarningsChanged(bool value) => ApplyFilters();
        partial void OnShowErrorsChanged(bool value) => ApplyFilters();

        private void ApplyFilters()
        {
            FilteredEntries.Clear();
            foreach (var e in AllEntries)
                if (PassesFilter(e))
                    FilteredEntries.Add(e);
        }

        private bool PassesFilter(LogEntry e)
        {
            if (!string.IsNullOrWhiteSpace(FilterText) &&
                !e.Message.Contains(FilterText, StringComparison.OrdinalIgnoreCase))
                return false;

            return (e.Severity == LogSeverity.Info && ShowInfo)
                || (e.Severity == LogSeverity.Warning && ShowWarnings)
                || (e.Severity == LogSeverity.Error && ShowErrors);
        }
    }
}