using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PulsePanel.App.Pages;

namespace PulsePanel.App
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            var vm = new MainViewModel(App.NavigationService);
            if (this.Content is FrameworkElement root)
            {
                root.DataContext = vm;
            }

            // Initialize navigation
            App.NavigationService.Initialize(MainFrame);
            App.NavigationService.Navigate(typeof(ServersPage));
        }
    }
}
