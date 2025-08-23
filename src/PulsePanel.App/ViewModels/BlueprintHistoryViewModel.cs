using CommunityToolkit.Mvvm.ComponentModel;
using PulsePanel.App.Models;
using PulsePanel.App.Services;
using System.Collections.ObjectModel;
using System.Linq;

namespace PulsePanel.App.ViewModels
{
    public partial class BlueprintHistoryViewModel : ObservableObject
    {
        private readonly IProvenanceHistoryService _historyService;

        [ObservableProperty] private ObservableCollection<ExecutionSession> sessions = new();
        [ObservableProperty] private ExecutionSession? selectedSession;

        public BlueprintHistoryViewModel(IProvenanceHistoryService historyService)
        {
            _historyService = historyService;
            LoadSessions();
        }

        private void LoadSessions()
        {
            Sessions = new ObservableCollection<ExecutionSession>(_historyService.GetAllSessions().OrderByDescending(s => s.StartedAt));
        }
    }
}