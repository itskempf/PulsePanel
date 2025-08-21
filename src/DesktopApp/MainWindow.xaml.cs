using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;

namespace PulsePanel.DesktopApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            NavView.ItemInvoked += OnItemInvoked;
            NavView.SelectedItem = NavView.MenuItems[0];
        }

        private void OnItemInvoked(NavigationView sender, NavigationViewItemInvokedEventArgs args)
        {
            if (args.IsSettingsInvoked)
            {
                ContentFrame.Navigate(typeof(Pages.SettingsPage));
            }
            else
            {
                var item = args.InvokedItemContainer;
                if (item != null)
                {
                    var type = Type.GetType(item.Tag.ToString());
                    ContentFrame.Navigate(type);
                }
            }
        }
    }
}
