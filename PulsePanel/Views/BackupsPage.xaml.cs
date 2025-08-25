
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.ViewModels;

namespace PulsePanel.Views
{
    public partial class BackupsPage : Page
    {
        public BackupsPage()
        {
            InitializeComponent();
            DataContext = PulsePanel.App.Services.GetRequiredService<BackupsViewModel>();
            _ = ((BackupsViewModel)DataContext).LoadAsync();
        }
    }
}
