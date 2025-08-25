
using System;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ContentFrame.Navigate(new Uri("Views/DashboardPage.xaml", UriKind.Relative));
        }
        private void Nav_Click(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string page)
                ContentFrame.Navigate(new Uri($"Views/{page}.xaml", UriKind.Relative));
        }
    }
}
