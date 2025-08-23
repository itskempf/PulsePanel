using Microsoft.UI.Xaml.Controls;
using PulsePanel.App.ViewModels;
using System.Collections.ObjectModel;

namespace PulsePanel.App.Pages
{
    public sealed partial class ServersHealthPage : Page
    {
        public ObservableCollection<ServerHealthViewModel> Servers { get; } = new();

        public ServersHealthPage()
        {
            this.InitializeComponent();
            DataContext = this;

            // Mock data for now
            Servers.Add(new ServerHealthViewModel
            {
                Name = "Build Server",
                Game = "MC",
                StatusText = "Running",
                CpuPercent = 5.2,
                RamPercent = 26.3,
                DiskPercent = 71.0,
                NetMbps = 2.0,
                Pid = 1234,
                Port = 25565,
                HeartbeatAge = 1.1,
                ServiceStatus = "Running",
                Uptime = System.TimeSpan.FromHours(2.3)
            });
        }
    }
}
