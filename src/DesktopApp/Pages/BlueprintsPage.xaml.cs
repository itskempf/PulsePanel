using Microsoft.UI.Xaml.Controls;
using PulsePanel.DesktopApp.Services;

namespace PulsePanel.DesktopApp.Pages
{
    public sealed partial class BlueprintsPage : Page
    {
        public BlueprintsPage()
        {
            this.InitializeComponent();
            var dataService = new DataService();
            this.DataContext = dataService.GetMockBlueprints();
        }
    }
}
