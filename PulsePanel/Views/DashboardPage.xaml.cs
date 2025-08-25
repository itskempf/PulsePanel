
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.ViewModels;

namespace PulsePanel.Views
{
    public partial class DashboardPage : Page
    {
        public DashboardPage()
        {
            InitializeComponent();
            DataContext = App.Services.GetRequiredService<ServersViewModel>();
            _ = ((ServersViewModel)DataContext).LoadAsync();
        }
    }
}
