using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePanel.App.Models;
using PulsePanel.App.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Linq; // Added
using System.Threading; // Added

namespace PulsePanel.App.ViewModels
{
    public partial class BlueprintCatalogViewModel : ObservableObject
    {
        private readonly BlueprintLoader _loader;
        private readonly IBlueprintExecutor _executor;
        private readonly IProvenanceHistoryService _history;
        private readonly IRemoteExecutionRouter _router; // Added
        private readonly INodeRegistry _nodes; // Added

        [ObservableProperty] private ObservableCollection<Blueprint> blueprints = new();
        [ObservableProperty] private Blueprint? selectedBlueprint;
        [ObservableProperty] private string statusMessage = "";
        [ObservableProperty] private bool isDryRun;
        public NodesViewModel NodesVM { get; } // Added

        public BlueprintCatalogViewModel(
            BlueprintLoader loader,
            IBlueprintExecutor executor,
            IProvenanceHistoryService history,
            IRemoteExecutionRouter router,
            INodeRegistry nodes,
            NodesViewModel nodesVM) // Added
        {
            _loader = loader;
            _executor = executor;
            _history = history;
            _router = router;
            _nodes = nodes;
            NodesVM = nodesVM;
            LoadBlueprints();
        }

        private void LoadBlueprints()
        {
            Blueprints = new ObservableCollection<Blueprint>(_loader.LoadAll());
        }

        [RelayCommand]
        public async Task InstallAsync()
        {
            if (SelectedBlueprint == null) return;
            StatusMessage = $"Installing {SelectedBlueprint.Name}...";
            var opts = new ExecutionOptions { DryRun = IsDryRun, TargetNodeId = NodesVM.SelectedNode?.Id, CancellationToken = CancellationToken.None }; // Modified
            await _router.RouteAsync(SelectedBlueprint, ExecutionActionType.Install, opts, CancellationToken.None); // Modified
            StatusMessage = $"Install requested for {SelectedBlueprint.Name}.";
        }

        [RelayCommand]
        public async Task UpdateAsync()
        {
            if (SelectedBlueprint == null) return;
            StatusMessage = $"Updating {SelectedBlueprint.Name}...";
            var opts = new ExecutionOptions { DryRun = IsDryRun, TargetNodeId = NodesVM.SelectedNode?.Id, CancellationToken = CancellationToken.None }; // Modified
            await _router.RouteAsync(SelectedBlueprint, ExecutionActionType.Update, opts, CancellationToken.None); // Modified
            StatusMessage = $"Update requested for {SelectedBlueprint.Name}.";
        }

        [RelayCommand]
        public async Task ValidateAsync()
        {
            if (SelectedBlueprint == null) return;
            StatusMessage = $"Validating {SelectedBlueprint.Name}...";
            var opts = new ExecutionOptions { DryRun = IsDryRun, TargetNodeId = NodesVM.SelectedNode?.Id, CancellationToken = CancellationToken.None }; // Modified
            await _router.RouteAsync(SelectedBlueprint, ExecutionActionType.Validate, opts, CancellationToken.None); // Modified
            StatusMessage = $"Validation requested for {SelectedBlueprint.Name}.";
        }

        [RelayCommand]
        private async Task ReplayLastAsync()
        {
            if (SelectedBlueprint is null) return;
            var last = _history.GetAllSessions()
                               .Where(s => s.BlueprintName == SelectedBlueprint.Name)
                               .OrderByDescending(s => s.StartedAt)
                               .FirstOrDefault();
            if (last is null) { StatusMessage = "No past sessions for selected blueprint."; return; }

            var opts = new ExecutionOptions { ReplaySessionId = last.Id, DryRun = IsDryRun, TargetNodeId = NodesVM.SelectedNode?.Id, CancellationToken = CancellationToken.None }; // Modified
            await _router.RouteAsync(SelectedBlueprint, last.Action, opts, CancellationToken.None); // Modified
        }

        partial void OnIsDryRunChanged(bool value)
        {
            StatusMessage = value ? "Dry‑Run enabled." : "";
        }
    }
}