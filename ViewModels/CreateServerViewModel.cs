using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePanel.Core.Services;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Pulse_Panel.Views;
using PulsePanel.Services;

namespace PulsePanel.ViewModels
{
    // NOTE: We will replace the simple blueprint model later
    public class SimpleBlueprint { public string Name { get; set; } = ""; }

    public partial class CreateServerViewModel : ObservableObject
    {
        private readonly IProvisioningService _provisioningService;
        private readonly IServerService _serverService;
        private readonly NavigationService _navigationService;

        // This would come from a real blueprint service later
        public ObservableCollection<SimpleBlueprint> Blueprints { get; } = new()
        {
            new SimpleBlueprint { Name = "Minecraft" },
            new SimpleBlueprint { Name = "Valheim" },
            new SimpleBlueprint { Name = "ARK: Survival Evolved" }
        };

        [ObservableProperty]
        private string _newServerName = string.Empty;

        [ObservableProperty]
        private int _newServerPort = 25565;
        
        [ObservableProperty]
        private SimpleBlueprint? _selectedBlueprint;

        public CreateServerViewModel(IProvisioningService provisioningService, IServerService serverService, NavigationService navigationService)
        {
            _provisioningService = provisioningService;
            _serverService = serverService;
            _navigationService = navigationService;
        }

        [RelayCommand]
        private async Task CreateServerAsync()
        {
            if (SelectedBlueprint == null || string.IsNullOrWhiteSpace(NewServerName))
            {
                // Add real validation/user feedback later
                return;
            }

            // 1. "Install" the server
            var newServer = await _provisioningService.ProvisionServerAsync(NewServerName, SelectedBlueprint.Name);

            // 2. Save the new server to our list
            await _serverService.AddNewServerAsync(newServer);

            // 3. Navigate back to the servers page
            _navigationService.RequestNavigate(typeof(ServersPage));
        }
    }
}