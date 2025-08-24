using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using PulsePanel.Services;
using Pulse_Panel.Views;

namespace Pulse_Panel
{
    public sealed partial class MainWindow : Window
    {
        private readonly NavigationService _navigationService;

        public MainWindow()
        {
            this.InitializeComponent();
            // Load the default page on startup
            ContentFrame.Navigate(typeof(Views.ServersPage));

            // Hook NavigationService to navigate the Frame
            _navigationService = PulsePanel.Services.ServiceLocator.Get<NavigationService>();
            _navigationService.NavigateRequested += OnNavigateRequested;
        }

        private void OnNavigateRequested(System.Type pageType)
        {
            if (ContentFrame.SourcePageType != pageType)
            {
                ContentFrame.Navigate(pageType);
            }
        }

        private void RootNavigationView_ItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Views.SettingsPage));
            }
            else if (args.InvokedItemContainer != null)
            {
                var tag = args.InvokedItemContainer.Tag.ToString();
                switch (tag)
                {
                    case "ServersPage":
                        ContentFrame.Navigate(typeof(Views.ServersPage));
                        break;
                    case "BlueprintsPage":
                        ContentFrame.Navigate(typeof(Views.BlueprintsPage));
                        break;
                    case "PluginsPage":
                        ContentFrame.Navigate(typeof(Views.PluginsPage));
                        break;
                    case "AuditLogPage":
                        ContentFrame.Navigate(typeof(Views.AuditLogPage));
                        break;
                }
            }
        }
    }
}