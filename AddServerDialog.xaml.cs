using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class AddServerDialog : Window
    {
        public GameServer? Server { get; private set; }

        public AddServerDialog()
        {
            InitializeComponent();
        }

        private void GameComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameComboBox.SelectedItem is ComboBoxItem item)
            {
                AppIdBox.Text = item.Tag?.ToString() ?? "";
                
                var gameName = item.Content?.ToString() ?? "";
                if (!string.IsNullOrEmpty(gameName))
                {
                    var defaultPath = Path.Combine(@"C:\GameServers", gameName.Replace(":", "").Replace(" ", ""));
                    InstallPathBox.Text = defaultPath;
                    
                    // Set default executable based on game
                    switch (item.Tag?.ToString())
                    {
                        case "730": // CS2
                            ExecutableBox.Text = Path.Combine(defaultPath, "game", "bin", "win64", "cs2.exe");
                            StartupArgsBox.Text = "-dedicated +map de_dust2";
                            break;
                        case "4020": // Garry's Mod
                            ExecutableBox.Text = Path.Combine(defaultPath, "srcds.exe");
                            StartupArgsBox.Text = "-console -game garrysmod +map gm_flatgrass +maxplayers 16";
                            break;
                        case "232250": // TF2
                            ExecutableBox.Text = Path.Combine(defaultPath, "srcds.exe");
                            StartupArgsBox.Text = "-console -game tf +map cp_dustbowl +maxplayers 24";
                            break;
                        case "222860": // L4D2
                            ExecutableBox.Text = Path.Combine(defaultPath, "srcds.exe");
                            StartupArgsBox.Text = "-console -game left4dead2 +map c1m1_hotel";
                            break;
                    }
                }
            }
        }

        private void BrowseInstallPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select Installation Directory",
                FileName = "Select Folder",
                DefaultExt = "folder",
                Filter = "Folder|*.folder"
            };

            if (dialog.ShowDialog() == true)
            {
                InstallPathBox.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
            }
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ServerNameBox.Text))
            {
                MessageBox.Show("Please enter a server name.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (GameComboBox.SelectedItem == null)
            {
                MessageBox.Show("Please select a game.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (!int.TryParse(PortBox.Text, out int port) || port <= 0 || port > 65535)
            {
                MessageBox.Show("Please enter a valid port number (1-65535).", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var selectedGame = (ComboBoxItem)GameComboBox.SelectedItem;
            
            Server = new GameServer
            {
                Name = ServerNameBox.Text.Trim(),
                GameName = selectedGame.Content?.ToString() ?? "",
                AppId = AppIdBox.Text.Trim(),
                InstallPath = InstallPathBox.Text.Trim(),
                ExecutablePath = ExecutableBox.Text.Trim(),
                StartupArgs = StartupArgsBox.Text.Trim(),
                Port = port
            };

            DialogResult = true;
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}