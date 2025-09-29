using Microsoft.Win32;
using System.IO;
using System.Windows;

namespace PulsePanel
{
    public partial class SettingsDialog : Window
    {
        public string SteamCmdPath { get; set; } = @"C:\steamcmd\steamcmd.exe";
        public string DefaultInstallPath { get; set; } = @"C:\GameServers";

        public SettingsDialog()
        {
            InitializeComponent();
            SteamCmdPathBox.Text = SteamCmdPath;
            DefaultInstallPathBox.Text = DefaultInstallPath;
        }

        private void BrowseSteamCmd_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select SteamCMD Executable",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                FileName = "steamcmd.exe"
            };

            if (dialog.ShowDialog() == true)
            {
                SteamCmdPathBox.Text = dialog.FileName;
            }
        }

        private void BrowseInstallPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select Default Install Directory",
                FileName = "Select Folder",
                DefaultExt = "folder",
                Filter = "Folder|*.folder"
            };

            if (dialog.ShowDialog() == true)
            {
                DefaultInstallPathBox.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(SteamCmdPathBox.Text))
            {
                MessageBox.Show("Please specify the SteamCMD path.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            SteamCmdPath = SteamCmdPathBox.Text.Trim();
            DefaultInstallPath = DefaultInstallPathBox.Text.Trim();
            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}