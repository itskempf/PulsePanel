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
            ModFilesListBox.Items.Clear();
            
            if (_modDirectory == null || !Directory.Exists(_modDirectory))
            {
                OutputReceived?.Invoke("No mod directory found");
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

            var workshopIds = WorkshopIdsBox.Text.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => id.Trim())
                .Where(id => !string.IsNullOrEmpty(id) && id.All(char.IsDigit))
                .ToList();

            if (!workshopIds.Any())
            {
                OutputReceived?.Invoke("No valid Workshop IDs found");
                return;
            }

            DownloadModsBtn.IsEnabled = false;
            OutputReceived?.Invoke($"Starting download of {workshopIds.Count} workshop items...");
            
            try
            {
                var steamCmdPath = @"C:\steamcmd\steamcmd.exe";
                if (!File.Exists(steamCmdPath))
                {
                    OutputReceived?.Invoke("SteamCMD not found at default location");
                    return;
                }

                var successCount = 0;
                foreach (var workshopId in workshopIds)
                {
                    OutputReceived?.Invoke($"Downloading Workshop ID: {workshopId}");
                    
                    var args = $"+login anonymous +workshop_download_item {_server.AppId} {workshopId} +quit";
                    
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = steamCmdPath,
                            Arguments = args,
                            UseShellExecute = false,
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            CreateNoWindow = true
                        }
                    };

                    process.OutputDataReceived += (s, e) => {
                        if (!string.IsNullOrEmpty(e.Data))
                            OutputReceived?.Invoke($"SteamCMD: {e.Data}");
                    };

                    process.Start();
                    process.BeginOutputReadLine();
                    await process.WaitForExitAsync();
                    
                    if (process.ExitCode == 0)
                    {
                        successCount++;
                        OutputReceived?.Invoke($"✓ Downloaded Workshop ID: {workshopId}");
                    }
                    else
                    {
                        OutputReceived?.Invoke($"✗ Failed to download Workshop ID: {workshopId}");
                    }
                }

                OutputReceived?.Invoke($"Workshop download completed: {successCount}/{workshopIds.Count} successful");
                new ToastNotification("Mods Downloaded", $"{successCount} workshop items downloaded successfully");
                RefreshModList();
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Download failed: {ex.Message}");
            }
            finally
            {
                DownloadModsBtn.IsEnabled = true;
            }
        }

        private void ClearMods_Click(object sender, RoutedEventArgs e)
        {
            WorkshopIdsBox.Clear();
            OutputReceived?.Invoke("Cleared workshop ID list");
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

        private async void ValidateMods_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;
            
            ValidateModsBtn.IsEnabled = false;
            OutputReceived?.Invoke("Validating mods...");
            
            try
            {
                var steamCmdPath = @"C:\steamcmd\steamcmd.exe";
                if (!File.Exists(steamCmdPath))
                {
                    OutputReceived?.Invoke("SteamCMD not found for validation");
                    return;
                }

                var args = $"+force_install_dir \"{_server.InstallPath}\" +login anonymous +app_update {_server.AppId} validate +quit";
                
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
                        OutputReceived?.Invoke($"Validation: {e.Data}");
                };

                process.Start();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();
                
                if (process.ExitCode == 0)
                {
                    OutputReceived?.Invoke("✓ Mod validation completed successfully");
                    new ToastNotification("Validation Complete", "Mod validation completed successfully");
                }
                else
                {
                    OutputReceived?.Invoke("✗ Mod validation failed");
                }
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Validation error: {ex.Message}");
            }
            finally
            {
                ValidateModsBtn.IsEnabled = true;
            }
        }
    }
}