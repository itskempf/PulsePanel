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
                Path.Combine(_server.InstallPath, "mods"),
                Path.Combine(_server.InstallPath, "plugins")
            };

            _modDirectory = possiblePaths.FirstOrDefault(Directory.Exists);
        }

        private void RefreshModList()
        {
            ModFilesListBox.Items.Clear();
            
            if (_modDirectory == null || !Directory.Exists(_modDirectory))
            {
                ModFilesListBox.Items.Add("No mod directory found");
                return;
            }

            try
            {
                var files = Directory.GetFileSystemEntries(_modDirectory);
                foreach (var file in files)
                {
                    var name = Path.GetFileName(file);
                    var isDirectory = Directory.Exists(file);
                    ModFilesListBox.Items.Add($"{(isDirectory ? "📁" : "📄")} {name}");
                }
            }
            catch (Exception ex)
            {
                ModFilesListBox.Items.Add($"Error: {ex.Message}");
            }
        }

        private async void DownloadMods_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null || string.IsNullOrWhiteSpace(WorkshopIdsBox.Text))
                return;

            var workshopIds = WorkshopIdsBox.Text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id) && id.All(char.IsDigit))
                .ToList();

            if (!workshopIds.Any())
            {
                MessageBox.Show("Please enter valid Workshop IDs", "Invalid Input", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DownloadModsBtn.IsEnabled = false;
            OutputReceived?.Invoke($"Downloading {workshopIds.Count} workshop items...");

            try
            {
                foreach (var id in workshopIds)
                {
                    await DownloadWorkshopItem(id);
                }
                
                RefreshModList();
                OutputReceived?.Invoke("Workshop download completed");
            }
            finally
            {
                DownloadModsBtn.IsEnabled = true;
            }
        }

        private async Task DownloadWorkshopItem(string workshopId)
        {
            var steamCmdPath = @"C:\steamcmd\steamcmd.exe"; // TODO: Get from settings
            
            if (!File.Exists(steamCmdPath))
            {
                OutputReceived?.Invoke("SteamCMD not found");
                return;
            }

            var args = $"+login anonymous +workshop_download_item {_server!.AppId} {workshopId} +quit";
            
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = steamCmdPath,
                    Arguments = args,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                }
            };

            process.OutputDataReceived += (s, e) => {
                if (!string.IsNullOrEmpty(e.Data))
                    OutputReceived?.Invoke(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            await process.WaitForExitAsync();
        }

        private void ClearMods_Click(object sender, RoutedEventArgs e)
        {
            WorkshopIdsBox.Clear();
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
                MessageBox.Show("Mod directory not found", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void ValidateMods_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;

            var workshopIds = WorkshopIdsBox.Text
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id) && id.All(char.IsDigit));

            foreach (var id in workshopIds)
            {
                OutputReceived?.Invoke($"Validating workshop item {id}...");
                // TODO: Implement validation logic
            }
        }
    }
}