using Microsoft.UI.Xaml.Controls;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App.Pages
{
    public sealed partial class ServersPage : Page
    {
        public ServersPage()
        {
            this.InitializeComponent();
            DataContext = App.Services.GetRequiredService<ServersViewModel>();
        }
    }
}
