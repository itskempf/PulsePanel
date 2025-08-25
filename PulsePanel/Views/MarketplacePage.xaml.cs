
using System.Windows;
using System.Windows.Controls;
using PulsePanel.ViewModels;
using PulsePanel.Core.Models;

namespace PulsePanel.Views
{
    public partial class MarketplacePage : Page
    {
        public MarketplacePage()
        {
            InitializeComponent();
            DataContext = App.Services.GetService(typeof(MarketplaceViewModel));
            _ = ((MarketplaceViewModel)DataContext!).LoadAsync();
        }
        private void Install_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is MarketplaceViewModel vm && vm.Selected is Blueprint bp)
            {
                var dlg = new CreateServerDialog(bp);
                dlg.Owner = System.Windows.Application.Current.MainWindow;
                dlg.ShowDialog();
            }
        }
    }
}
