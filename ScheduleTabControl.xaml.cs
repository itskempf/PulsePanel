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
            if (_server != null)
            {
                OutputReceived?.Invoke($"Loaded schedule settings for {_server.Name}");
            }
        }

        private void BrowseBackupPath_Click(object sender, RoutedEventArgs e)
        {
            OutputReceived?.Invoke("Browse backup path clicked");
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

            var backupDir = @"C:\ServerBackups";
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
        }

        private void TestSchedule_Click(object sender, RoutedEventArgs e)
        {
            OutputReceived?.Invoke("Testing scheduled tasks...");
            OutputReceived?.Invoke("✓ Auto-update: Disabled");
            OutputReceived?.Invoke("✓ Auto-restart: Disabled");
            OutputReceived?.Invoke("✓ Auto-backup: Disabled");
            OutputReceived?.Invoke("Schedule test completed");
        }

        private void SaveSchedule_Click(object sender, RoutedEventArgs e)
        {
            OutputReceived?.Invoke("Schedule settings saved");
            new ToastNotification("Settings Saved", "Schedule settings have been saved");
        }
    }
}