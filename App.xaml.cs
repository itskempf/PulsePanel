using System.Configuration;
using System.Data;
using System.Windows;
using System.Windows.Threading;

namespace PulsePanel
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            SetupExceptionHandling();
            HandleStartupArgs(e);
            base.OnStartup(e);
        }
        
        private void HandleStartupArgs(StartupEventArgs e)
        {
            var args = e.Args;
            
            if (args.Length > 0)
            {
                // Handle command line arguments for scheduled tasks
                if (args[0] == "--update-server" && args.Length > 1)
                {
                    HandleUpdateServer(args[1]);
                    Shutdown();
                    return;
                }
                else if (args[0] == "--restart-server" && args.Length > 1)
                {
                    HandleRestartServer(args[1]);
                    Shutdown();
                    return;
                }
                else if (args[0] == "--backup-server" && args.Length > 1)
                {
                    HandleBackupServer(args[1]);
                    Shutdown();
                    return;
                }
            }
        }
        
        private async void HandleUpdateServer(string serverId)
        {
            try
            {
                var settings = SettingsManager.LoadSettings();
                var server = settings.Servers.FirstOrDefault(s => s.Id == serverId);
                
                if (server != null)
                {
                    var steamCmd = new SteamCmdManager(settings.SteamCmdPath);
                    await steamCmd.InstallOrUpdateServerAsync(server);
                }
            }
            catch { }
        }
        
        private async void HandleRestartServer(string serverId)
        {
            try
            {
                var settings = SettingsManager.LoadSettings();
                var server = settings.Servers.FirstOrDefault(s => s.Id == serverId);
                
                if (server != null)
                {
                    var steamCmd = new SteamCmdManager(settings.SteamCmdPath);
                    steamCmd.StopServer(server);
                    await Task.Delay(5000);
                    steamCmd.StartServer(server);
                }
            }
            catch { }
        }
        
        private async void HandleBackupServer(string serverId)
        {
            try
            {
                var settings = SettingsManager.LoadSettings();
                var server = settings.Servers.FirstOrDefault(s => s.Id == serverId);
                
                if (server != null)
                {
                    var backupPath = @"C:\ServerBackups";
                    await BackupManager.CreateFullBackup(server, backupPath);
                    await BackupManager.CleanOldBackups(backupPath, server.Name, 30);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Failed to handle backup server command: {serverId}", ex);
            }
        }
        
        private void SetupExceptionHandling()
        {
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                Logger.LogError("Unhandled domain exception", ex);
                
                if (e.IsTerminating)
                {
                    MessageBox.Show($"A critical error occurred: {ex?.Message}\n\nThe application will now close.", 
                        "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            };
            
            DispatcherUnhandledException += (s, e) =>
            {
                Logger.LogError("Unhandled UI exception", e.Exception);
                
                MessageBox.Show($"An error occurred: {e.Exception.Message}\n\nThe operation has been cancelled.", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                
                e.Handled = true;
            };
            
            System.Threading.Tasks.TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Logger.LogError("Unhandled task exception", e.Exception);
                e.SetObserved();
            };
        }
    }
}
