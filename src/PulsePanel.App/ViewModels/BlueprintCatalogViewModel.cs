using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePanel.App.Models;
using PulsePanel.App.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace PulsePanel.App.ViewModels
{
    public partial class BlueprintCatalogViewModel : ObservableObject
    {
        private readonly BlueprintLoader _loader;
        private readonly IBlueprintExecutor _executor;

        [ObservableProperty] private ObservableCollection<Blueprint> blueprints = new();
        [ObservableProperty] private Blueprint? selectedBlueprint;
        [ObservableProperty] private string statusMessage = "";

        public BlueprintCatalogViewModel(BlueprintLoader loader, IBlueprintExecutor executor)
        {
            _loader = loader;
            _executor = executor;
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
            await _executor.ExecuteInstallAsync(SelectedBlueprint);
            StatusMessage = $"Install complete for {SelectedBlueprint.Name}.";
        }

        [RelayCommand]
        public async Task UpdateAsync()
        {
            if (SelectedBlueprint == null) return;
            StatusMessage = $"Updating {SelectedBlueprint.Name}...";
            await _executor.ExecuteUpdateAsync(SelectedBlueprint);
            StatusMessage = $"Update complete for {SelectedBlueprint.Name}.";
        }

        [RelayCommand]
        public async Task ValidateAsync()
        {
            if (SelectedBlueprint == null) return;
            StatusMessage = $"Validating {SelectedBlueprint.Name}...";
            await _executor.ExecuteValidateAsync(SelectedBlueprint);
            StatusMessage = $"Validation complete for {SelectedBlueprint.Name}.";
        }
    }
}