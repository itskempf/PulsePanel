using Microsoft.UI.Xaml.Controls;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App.Pages
{
    public sealed partial class AnalyticsPage : Page
    {
        public ServersViewModel ViewModel { get; }

        public AnalyticsPage()
        {
            this.InitializeComponent();
            ViewModel = App.GetService<ServersViewModel>();
        }
    }
}
