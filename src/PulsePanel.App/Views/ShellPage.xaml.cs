using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace PulsePanel.App.Views
{
    public sealed partial class ShellPage : Page
    {
        public ShellPage() { InitializeComponent(); Nav.SelectionChanged += OnSelectionChanged; Loaded += OnLoaded; }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            Nav.SelectedItem = Nav.MenuItems[0];
            Navigate("ControlCenter");
        }

        private void OnSelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            if (args.SelectedItemContainer is NavigationViewItem item && item.Tag is string tag)
                Navigate(tag);
        }

        private void Navigate(string tag)
        {
            switch (tag)
            {
                case "ControlCenter": ContentFrame.Navigate(typeof(BlueprintControlCenterPage)); break;
                case "Queue": ContentFrame.Navigate(typeof(JobQueuePage)); break;
                case "Nodes": ContentFrame.Navigate(typeof(NodesPage)); break;
                case "Compliance": ContentFrame.Navigate(typeof(ComplianceDashboardPage)); break;
            }
        }
    }
}