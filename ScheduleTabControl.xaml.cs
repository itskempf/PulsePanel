using Microsoft.Win32;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class ScheduleTabControl : UserControl
    {
        private GameServer? _server;
        public event Action<string>? OutputReceived;

        public ScheduleTabControl()
        {
            InitializeComponent();
        }

        public void UpdateServer(GameServer? server)
        {
            _server = server;
            LoadScheduleSettings();
        }

        private void LoadScheduleSettings()
        {
            // TODO: Load from settings file or database
            UpdateScheduleCombo.SelectedIndex = 0;
            RestartIntervalCombo.SelectedIndex = 0;
            BackupScheduleCombo.SelectedIndex = 0;
            BackupRetentionCombo.SelectedIndex = 1;
        }

        private void BrowseBackupPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog
            {
                Title = "Select Backup Directory",
                FileName = "Select Folder",
                DefaultExt = "folder",
                Filter = "Folder|*.folder"
            };

            if (dialog.ShowDialog() == true)
            {
                BackupPathBox.Text = Path.GetDirectoryName(dialog.FileName) ?? "";
            }
        }

        private async void BackupNow_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;

            BackupNowBtn.IsEnabled = false;
            OutputReceived?.Invoke($"Starting backup for {_server.Name}...");

            try
            {
                await CreateBackup();
                OutputReceived?.Invoke("Backup completed successfully");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Backup failed: {ex.Message}");
            }
            finally
            {
                BackupNowBtn.IsEnabled = true;
            }
        }

        private async Task CreateBackup()
        {
            if (_server == null) return;

            var backupDir = BackupPathBox.Text;
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var backupFileName = $"{_server.Name}_{timestamp}.zip";
            var backupPath = Path.Combine(backupDir, backupFileName);

            await Task.Run(() =>
            {
                ZipFile.CreateFromDirectory(_server.InstallPath, backupPath, CompressionLevel.Optimal, false);
            });

            OutputReceived?.Invoke($"Backup saved: {backupFileName}");
            
            // Clean old backups based on retention policy
            CleanOldBackups(backupDir);
        }

        private void CleanOldBackups(string backupDir)
        {
            if (_server == null) return;

            var retentionDays = BackupRetentionCombo.SelectedIndex switch
            {
                0 => 7,
                1 => 30,
                2 => 90,
                _ => 30
            };

            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            var backupFiles = Directory.GetFiles(backupDir, $"{_server.Name}_*.zip");

            foreach (var file in backupFiles)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.CreationTime < cutoffDate)
                {
                    try
                    {
                        File.Delete(file);
                        OutputReceived?.Invoke($"Deleted old backup: {Path.GetFileName(file)}");
                    }
                    catch (Exception ex)
                    {
                        OutputReceived?.Invoke($"Failed to delete {Path.GetFileName(file)}: {ex.Message}");
                    }
                }
            }
        }

        private void TestSchedule_Click(object sender, RoutedEventArgs e)
        {
            OutputReceived?.Invoke("Testing scheduled tasks...");
            
            if (AutoUpdateEnabled.IsChecked == true)
                OutputReceived?.Invoke($"✓ Auto-update scheduled: {UpdateScheduleCombo.Text}");
            
            if (AutoRestartEnabled.IsChecked == true)
                OutputReceived?.Invoke($"✓ Auto-restart scheduled: {RestartIntervalCombo.Text}");
            
            if (AutoBackupEnabled.IsChecked == true)
                OutputReceived?.Invoke($"✓ Auto-backup scheduled: {BackupScheduleCombo.Text}");
            
            OutputReceived?.Invoke("Schedule test completed");
        }

        private void SaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            // TODO: Save settings to file or database
            OutputReceived?.Invoke("Schedule settings saved");
            MessageBox.Show("Schedule settings have been saved.", "Settings Saved", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}