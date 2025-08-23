using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.App.ViewModels;
using PulsePanel.App.Pages;

namespace PulsePanel.App.Pages
{
    public sealed partial class ServersPage : Page
    {
        public ServersPage()
        {
            this.InitializeComponent();
            DataContext = App.Services.GetRequiredService<ServersViewModel>();
        }

        private void ViewAnalytics_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AnalyticsPage));
        }
    }
}
