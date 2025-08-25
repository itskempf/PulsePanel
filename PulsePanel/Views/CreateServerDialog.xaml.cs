
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.ViewModels;
using PulsePanel.Core.Models;

namespace PulsePanel.Views
{
    public partial class CreateServerDialog : Window
    {
        public CreateServerDialog(Blueprint bp)
        {
            InitializeComponent();
            DataContext = new CreateServerDialogViewModel(
                PulsePanel.App.Services.GetRequiredService<PulsePanel.Core.Services.IProvisioningService>(),
                PulsePanel.App.Services.GetRequiredService<PulsePanel.Core.Services.IServerService>(),
                PulsePanel.App.Services.GetRequiredService<PulsePanel.Core.Services.IBlueprintService>(),
                bp);
        }
        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is CreateServerDialogViewModel vm)
            {
                await vm.Create.ExecuteAsync();
                MessageBox.Show("Server created.");
                this.Close();
            }
        }
        private void Close_Click(object sender, RoutedEventArgs e) => this.Close();
    }
}
