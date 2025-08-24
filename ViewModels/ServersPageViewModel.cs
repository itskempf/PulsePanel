using PulsePanel.Core.Services;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Pulse_Panel.ViewModels
{
    public partial class ServersPageViewModel : BaseViewModel
    {
        private readonly IServerService _serverService;

        public ObservableCollection<ServerViewModel> Servers { get; } = new();

        public ServersPageViewModel()
        {
            // This is not ideal. In a real application, we would use dependency injection.
            _serverService = new ServerService(new ServerInstanceRepository());
            
            _ = LoadServersAsync();
        }

        private async Task LoadServersAsync()
        {                                
            var serverInstances = await _serverService.GetAllServersAsync();

            Servers.Clear();

            foreach (var serverInstance in serverInstances)
            {
                Servers.Add(new ServerViewModel
                {
                    ServerName = serverInstance.InstanceName,
                    GameName = serverInstance.GameName,
                    Status = serverInstance.Status.ToString(),
                    HealthScore = serverInstance.HealthScore,
                    // Placeholder icon path
                    GameIconPath = "Assets/StoreLogo.png"
                });
            }
        }
    }
}
