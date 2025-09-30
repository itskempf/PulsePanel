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
            UpdateScheduleCombo.SelectedIndex = 0;
            RestartIntervalCombo.SelectedIndex = 2;
            BackupScheduleCombo.SelectedIndex = 0;
            BackupRetentionCombo.SelectedIndex = 1;
        }

        public void UpdateServer(GameServer? server)
        {
            _server = server;
            LoadScheduleSettings();
        }

        private void LoadScheduleSettings()
        {
            if (_server != null)
            {
                OutputReceived?.Invoke($"Loaded schedule settings for {_server.Name}");
                RefreshBackups_Click(null, null);
            }
        }

        private void BrowseBackupPath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Select Backup Directory",
                FileName = "Select Folder",
                DefaultExt = "folder",
                Filter = "Folder|*.folder"
            };

            if (dialog.ShowDialog() == true)
            {
                var selectedPath = System.IO.Path.GetDirectoryName(dialog.FileName);
                if (!string.IsNullOrEmpty(selectedPath))
                {
                    BackupPathBox.Text = selectedPath;
                    OutputReceived?.Invoke($"Backup path set to: {selectedPath}");
                }
            }
        }

        private async void BackupNow_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;

            OutputReceived?.Invoke($"Starting backup for {_server.Name}...");

            try
            {
                await CreateBackup();
                OutputReceived?.Invoke("Backup completed successfully");
                new ToastNotification("Backup Complete", "Server backup created successfully");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Backup failed: {ex.Message}");
            }
        }

        private async Task CreateBackup()
        {
            if (_server == null) return;

            var backupDir = BackupPathBox.Text;
            if (!Directory.Exists(backupDir))
                Directory.CreateDirectory(backupDir);

            try
            {
                var backups = BackupManager.GetBackupList(backupDir, _server.Name);
                var lastFullBackup = backups.FirstOrDefault(b => !b.IsIncremental);
                
                string backupFileName;
                
                // Create incremental backup if last full backup is less than 7 days old
                if (lastFullBackup != null && (DateTime.Now - lastFullBackup.BackupDate).TotalDays < 7)
                {
                    backupFileName = await BackupManager.CreateIncrementalBackup(_server, backupDir, lastFullBackup.BackupDate);
                    OutputReceived?.Invoke($"Incremental backup saved: {backupFileName}");
                }
                else
                {
                    backupFileName = await BackupManager.CreateFullBackup(_server, backupDir);
                    OutputReceived?.Invoke($"Full backup saved: {backupFileName}");
                }
                
                // Clean old backups based on retention policy
                await CleanOldBackups(backupDir);
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Backup failed: {ex.Message}");
                throw;
            }
        }
        
        private async Task CleanOldBackups(string backupDir)
        {
            try
            {
                var retentionDays = ((ComboBoxItem)BackupRetentionCombo.SelectedItem).Content.ToString() switch
                {
                    "7 days" => 7,
                    "30 days" => 30,
                    "90 days" => 90,
                    _ => 30
                };
                
                var backupsBefore = BackupManager.GetBackupList(backupDir, _server!.Name).Count;
                await BackupManager.CleanOldBackups(backupDir, _server.Name, retentionDays);
                var backupsAfter = BackupManager.GetBackupList(backupDir, _server.Name).Count;
                
                var deletedCount = backupsBefore - backupsAfter;
                if (deletedCount > 0)
                {
                    OutputReceived?.Invoke($"Cleaned {deletedCount} old backup(s)");
                }
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error cleaning old backups: {ex.Message}");
            }
        }

        private async void TestSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;
            
            OutputReceived?.Invoke("Testing scheduled tasks...");
            
            var updateTaskName = $"PulsePanel_Update_{_server.Name}";
            var restartTaskName = $"PulsePanel_Restart_{_server.Name}";
            var backupTaskName = $"PulsePanel_Backup_{_server.Name}";
            
            var updateExists = await TaskScheduler.TaskExists(updateTaskName);
            var restartExists = await TaskScheduler.TaskExists(restartTaskName);
            var backupExists = await TaskScheduler.TaskExists(backupTaskName);
            
            OutputReceived?.Invoke($"✓ Auto-update: {(updateExists ? "Enabled" : "Disabled")}");
            OutputReceived?.Invoke($"✓ Auto-restart: {(restartExists ? "Enabled" : "Disabled")}");
            OutputReceived?.Invoke($"✓ Auto-backup: {(backupExists ? "Enabled" : "Disabled")}");
            OutputReceived?.Invoke("Schedule test completed");
        }

        private async void SaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;
            
            SaveScheduleBtn.IsEnabled = false;
            OutputReceived?.Invoke("Saving schedule settings...");
            
            try
            {
                var appPath = System.Reflection.Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe");
                
                // Auto-update scheduling
                var updateTaskName = $"PulsePanel_Update_{_server.Name}";
                await TaskScheduler.DeleteScheduledTask(updateTaskName);
                
                if (AutoUpdateEnabled.IsChecked == true)
                {
                    var updateSchedule = ((ComboBoxItem)UpdateScheduleCombo.SelectedItem).Content.ToString();
                    var updateArgs = $"--update-server \"{_server.Id}\"";
                    
                    var success = await TaskScheduler.CreateScheduledTask(updateTaskName, updateSchedule!, appPath, updateArgs);
                    OutputReceived?.Invoke(success ? "✓ Auto-update scheduled" : "✗ Failed to schedule auto-update");
                }
                
                // Auto-restart scheduling
                var restartTaskName = $"PulsePanel_Restart_{_server.Name}";
                await TaskScheduler.DeleteScheduledTask(restartTaskName);
                
                if (AutoRestartEnabled.IsChecked == true)
                {
                    var restartInterval = ((ComboBoxItem)RestartIntervalCombo.SelectedItem).Content.ToString();
                    var restartArgs = $"--restart-server \"{_server.Id}\"";
                    
                    var success = await TaskScheduler.CreateScheduledTask(restartTaskName, restartInterval!, appPath, restartArgs);
                    OutputReceived?.Invoke(success ? "✓ Auto-restart scheduled" : "✗ Failed to schedule auto-restart");
                }
                
                // Auto-backup scheduling
                var backupTaskName = $"PulsePanel_Backup_{_server.Name}";
                await TaskScheduler.DeleteScheduledTask(backupTaskName);
                
                if (AutoBackupEnabled.IsChecked == true)
                {
                    var backupSchedule = ((ComboBoxItem)BackupScheduleCombo.SelectedItem).Content.ToString();
                    var backupArgs = $"--backup-server \"{_server.Id}\"";
                    
                    var success = await TaskScheduler.CreateScheduledTask(backupTaskName, backupSchedule!, appPath, backupArgs);
                    OutputReceived?.Invoke(success ? "✓ Auto-backup scheduled" : "✗ Failed to schedule auto-backup");
                }
                
                OutputReceived?.Invoke("Schedule settings saved");
                new ToastNotification("Settings Saved", "Schedule settings have been saved");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error saving schedule: {ex.Message}");
            }
            finally
            {
                SaveScheduleBtn.IsEnabled = true;
            }
        }
        
        private void RefreshBackups_Click(object? sender, RoutedEventArgs? e)
        {
            if (_server == null) return;
            
            try
            {
                var backupPath = BackupPathBox.Text;
                var backups = BackupManager.GetBackupList(backupPath, _server.Name);
                BackupListBox.ItemsSource = backups;
                OutputReceived?.Invoke($"Found {backups.Count} backup(s)");
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error loading backups: {ex.Message}");
            }
        }
        
        private async void RestoreBackup_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null || BackupListBox.SelectedItem is not BackupInfo selectedBackup) return;
            
            var result = MessageBox.Show(
                $"Are you sure you want to restore backup '{selectedBackup.FileName}'?\n\nThis will replace all current server files!",
                "Confirm Restore",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);
                
            if (result != MessageBoxResult.Yes) return;
            
            RestoreBackupBtn.IsEnabled = false;
            
            try
            {
                OutputReceived?.Invoke($"Restoring backup: {selectedBackup.FileName}");
                
                if (_server.Status == ServerStatus.Running)
                {
                    var steamCmd = new SteamCmdManager();
                    steamCmd.StopServer(_server);
                    await Task.Delay(3000);
                }
                
                var success = await BackupManager.RestoreBackup(selectedBackup.FilePath, _server.InstallPath);
                
                if (success)
                {
                    OutputReceived?.Invoke($"✓ Backup restored successfully: {selectedBackup.FileName}");
                    new ToastNotification("Restore Complete", "Backup restored successfully");
                }
                else
                {
                    OutputReceived?.Invoke($"✗ Failed to restore backup: {selectedBackup.FileName}");
                }
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Restore error: {ex.Message}");
            }
            finally
            {
                RestoreBackupBtn.IsEnabled = true;
            }
        }
    }
}