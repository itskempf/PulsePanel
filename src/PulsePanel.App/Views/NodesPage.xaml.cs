using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PulsePanel.App.Views
{
    public sealed partial class NodesPage : Page
    {
        public NodesAdminViewModel ViewModel => (NodesAdminViewModel)DataContext;
        public NodesPage() { InitializeComponent(); }

        private void OnAdd(object sender, RoutedEventArgs e)
        {
            ViewModel.Add(ApiKeyBox.Password);
            ApiKeyBox.Password = "";
        }
    }
}