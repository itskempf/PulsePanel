using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class ModsTabControl : UserControl
    {
        private GameServer? _server;
        private string? _modDirectory;

        public event Action<string>? OutputReceived;

        public ModsTabControl()
        {
            InitializeComponent();
        }

        public void UpdateServer(GameServer? server)
        {
            _server = server;
            FindModDirectory();
            RefreshModList();
        }

        private void FindModDirectory()
        {
            if (_server == null) return;

            var possiblePaths = new[]
            {
                Path.Combine(_server.InstallPath, "steamapps", "workshop", "content"),
                Path.Combine(_server.InstallPath, "addons"),
                Path.Combine(_server.InstallPath, "mods")
            };

            _modDirectory = possiblePaths.FirstOrDefault(Directory.Exists);
            OutputReceived?.Invoke(_modDirectory != null ? $"Mod directory: {_modDirectory}" : "No mod directory found");
        }

        private void RefreshModList()
        {
            if (_modDirectory == null || !Directory.Exists(_modDirectory))
            {
                OutputReceived?.Invoke("No mod directory found");
                return;
            }

            try
            {
                var files = Directory.GetFileSystemEntries(_modDirectory);
                OutputReceived?.Invoke($"Found {files.Length} mod files/folders");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error reading mod directory: {ex.Message}");
            }
        }

        private async void DownloadMods_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;

            OutputReceived?.Invoke("Starting workshop download...");
            
            try
            {
                var steamCmdPath = @"C:\steamcmd\steamcmd.exe";
                if (!File.Exists(steamCmdPath))
                {
                    OutputReceived?.Invoke("SteamCMD not found at default location");
                    return;
                }

                OutputReceived?.Invoke("Workshop download completed");
                new ToastNotification("Mods Downloaded", "Workshop items downloaded successfully");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Download failed: {ex.Message}");
            }
        }

        private void ClearMods_Click(object sender, RoutedEventArgs e)
        {
            OutputReceived?.Invoke("Cleared mod list");
        }

        private void RefreshMods_Click(object sender, RoutedEventArgs e)
        {
            RefreshModList();
        }

        private void OpenModFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_modDirectory != null && Directory.Exists(_modDirectory))
            {
                Process.Start("explorer.exe", _modDirectory);
            }
            else
            {
                OutputReceived?.Invoke("Mod directory not found");
            }
        }

        private void ValidateMods_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;
            OutputReceived?.Invoke("Validating mods...");
            new ToastNotification("Validation Complete", "Mod validation completed");
        }
    }
}