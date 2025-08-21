using Microsoft.UI.Xaml.Controls;
using PulsePanel.DesktopApp.Services;

namespace PulsePanel.DesktopApp.Pages
{
    public sealed partial class ServerListPage : Page
    {
        public ServerListPage()
        {
            this.InitializeComponent();
            var dataService = new DataService();
            this.DataContext = dataService.GetMockServers();
        }
    }
}
