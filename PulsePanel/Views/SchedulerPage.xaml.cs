
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.ViewModels;

namespace PulsePanel.Views
{
    public partial class SchedulerPage : Page
    {
        public SchedulerPage()
        {
            InitializeComponent();
            DataContext = PulsePanel.App.Services.GetRequiredService<SchedulerViewModel>();
            _ = ((SchedulerViewModel)DataContext).LoadAsync();
        }
    }
}
