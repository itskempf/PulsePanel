using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Pulse_Panel.Views
{
    public sealed partial class ServersPage : Page
    {
        public ServersPage()
        {
            this.InitializeComponent();
        }

        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Navigation to CreateServerPage can be wired through a navigation service.
            // For now, do nothing to avoid referencing removed App.RequestNavigation.
        }
    }
}