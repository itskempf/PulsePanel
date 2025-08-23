using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.App.Models;
using PulsePanel.App.Services;

namespace PulsePanel.App.ViewModels
{
    public partial class ComplianceDashboardViewModel : ObservableObject
    {
        private readonly IComplianceService _compliance;
        private readonly ISelfHealingOrchestrator _healer;
        private readonly BlueprintLoader _loader;
        private readonly INodeRegistry _nodes;

        [ObservableProperty] private bool selfHealingEnabled;
        public ObservableCollection<ComplianceReport> Reports { get; } = new();

        public ComplianceDashboardViewModel(IComplianceService compliance, ISelfHealingOrchestrator healer, BlueprintLoader loader, INodeRegistry nodes)
        {
            _compliance = compliance; _healer = healer; _loader = loader; _nodes = nodes;
        }

        [RelayCommand]
        private async Task ScanAllAsync()
        {
            Reports.Clear();
            foreach (var bp in _loader.LoadAll())
            {
                // Local scan; extend to per‑node as needed
                var rep = await _compliance.ScanAsync(bp, null, CancellationToken.None);
                Reports.Add(rep);
            }
        }

        partial void OnSelfHealingEnabledChanged(bool value) => _healer.Enabled = value;
    }
}