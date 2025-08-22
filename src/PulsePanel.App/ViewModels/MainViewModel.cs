using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PulsePanel.App.Services;
using PulsePanel.App.Pages;

namespace PulsePanel.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Server _selectedServer;

    public ObservableCollection<Server> Servers { get; set; }

    // Navigation
    private readonly INavigationService _navigationService;
    public ICommand NavigateServersCommand { get; }
    public ICommand NavigateBlueprintsCommand { get; }
    public ICommand NavigatePluginsCommand { get; }
    public ICommand NavigateAuditLogCommand { get; }
    public ICommand NavigateSettingsCommand { get; }

        public Server SelectedServer
        {
            get => _selectedServer;
            set
            {
                _selectedServer = value;
                OnPropertyChanged();
            }
        }

        public MainViewModel() : this(null) { }

        public MainViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;

            Servers = new ObservableCollection<Server>
            {
                new Server { Name="Minecraft", AppId="740", Status="Running", Path=@"C:\Servers\Minecraft", Game="Minecraft", HealthScore="ab93", Capabilities="R, Q", Blueprint="Vanilla <1.21.2>", Ports="TCP: 25565" },
                new Server { Name="CS.GO Server #3", AppId="740", Status="Running", Path=@"C:\Servers\CSGO3", Game="CS:GO", HealthScore="bb12", Capabilities="R, Q", Blueprint="Competitive", Ports="UDP: 27015" },
                new Server { Name="ARK: Survival Evolved", Status="Stopped", Path=@"C:\Servers\ARK", Game="ARK", HealthScore="n/a", Capabilities="", Blueprint="Default", Ports="TCP: 7777" },
                new Server { Name="Valheim", Status="Stopped", Path=@"C:\Servers\Valheim", Game="Valheim", HealthScore="n/a", Capabilities="", Blueprint="Default", Ports="UDP: 2456" },
                new Server { Name="Rust", Status="Running", Path=@"C:\Servers\Rust", Game="Rust", HealthScore="cc44", Capabilities="R", Blueprint="Vanilla", Ports="UDP: 28015" },
                new Server { Name="Unturned", Status="Running", Path=@"C:\Servers\Unturned", Game="Unturned", HealthScore="dd55", Capabilities="R, Q", Blueprint="Survival", Ports="TCP: 27015" },
                new Server { Name="V Rising", Status="Stopped", Path=@"C:\Servers\VRising", Game="V Rising", HealthScore="n/a", Capabilities="", Blueprint="Default", Ports="UDP: 9876" }
            };

            SelectedServer = Servers[0];

            // Commands
            NavigateServersCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(ServersPage)));
            NavigateBlueprintsCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(BlueprintsPage)));
            NavigatePluginsCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(PluginsPage)));
            NavigateAuditLogCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(AuditLogPage)));
            NavigateSettingsCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(SettingsPage)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class Server
    {
        public string Name { get; set; }
        public string AppId { get; set; }
        public string Status { get; set; }
        public string Path { get; set; }
        public string Game { get; set; }
        public string HealthScore { get; set; }
        public string Capabilities { get; set; }
        public string Blueprint { get; set; }
        public string Ports { get; set; }
    }
}
