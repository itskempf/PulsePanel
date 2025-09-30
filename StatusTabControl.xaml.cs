using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace PulsePanel
{
    public partial class StatusTabControl : UserControl, IDisposable
    {
        private GameServer? _server;
        private readonly ProcessMonitor _processMonitor = new();
        private DateTime _serverStartTime;
        private readonly DispatcherTimer _uptimeTimer = new();
        private readonly DispatcherTimer _diskTimer = new();
        private bool _disposed = false;

        public event Action<string>? OutputReceived;

        public StatusTabControl()
        {
            InitializeComponent();
            _processMonitor.ResourcesUpdated += OnResourcesUpdated;
            _processMonitor.ProcessCrashed += OnProcessCrashed;
            
            _uptimeTimer.Interval = TimeSpan.FromSeconds(1);
            _uptimeTimer.Tick += UpdateUptime;
            
            _diskTimer.Interval = TimeSpan.FromSeconds(30);
            _diskTimer.Tick += UpdateDiskUsage;
        }

        public void UpdateServer(GameServer? server)
        {
            if (_server != server)
            {
                _processMonitor.StopMonitoring();
                _uptimeTimer.Stop();
                _diskTimer.Stop();
                _server = server;
                
                if (_server?.Status == ServerStatus.Running)
                {
                    _serverStartTime = DateTime.Now;
                    _processMonitor.StartMonitoring(_server);
                    _uptimeTimer.Start();
                    _diskTimer.Start();
                }
            }
            
            UpdateDisplay();
            UpdateDiskUsage(null, null);
        }

        private void OnResourcesUpdated(GameServer server, float cpuPercent, long ramBytes)
        {
            Dispatcher.Invoke(() =>
            {
                try
                {
                    var ramMB = ramBytes / (1024 * 1024);
                    
                    CpuText.Text = $"{cpuPercent:F1}%";
                    RamText.Text = $"{ramMB} MB";
                    CpuProgressBar.Value = Math.Min(cpuPercent, 100);
                    RamProgressBar.Value = Math.Min((ramMB / 1024.0) * 100, 100);
                    
                    // Color-code based on usage levels
                    CpuProgressBar.Foreground = cpuPercent > 80 ? Brushes.Red : cpuPercent > 60 ? Brushes.Orange : Brushes.Green;
                    RamProgressBar.Foreground = ramMB > 2048 ? Brushes.Red : ramMB > 1024 ? Brushes.Orange : Brushes.Green;
                    
                    if (server.Status == ServerStatus.Running)
                    {
                        var uptime = DateTime.Now - _serverStartTime;
                        UptimeText.Text = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
                    }
                    
                    // Check alerts
                    AlertSystem.CheckAlerts(server, cpuPercent, ramBytes);
                }
                catch (Exception ex)
                {
                    Logger.LogWarning($"Resource update error: {ex.Message}");
                }
            });
        }

        private void OnProcessCrashed(GameServer server)
        {
            Dispatcher.Invoke(() =>
            {
                _uptimeTimer.Stop();
                _diskTimer.Stop();
                new ToastNotification("Server Crashed!", $"{server.Name} has stopped unexpectedly", true);
                OutputReceived?.Invoke($"CRASH DETECTED: {server.Name} process terminated unexpectedly");
                
                // Auto-restart if enabled
                if (server.AutoRestart)
                {
                    OutputReceived?.Invoke($"Auto-restarting {server.Name} in 5 seconds...");
                    Task.Delay(5000).ContinueWith(_ => 
                    {
                        var steamCmd = new SteamCmdManager();
                        steamCmd.StartServer(server);
                    });
                }
                
                UpdateDisplay();
            });
        }



        private void UpdateDisplay()
        {
            if (_server == null)
            {
                ServerNameText.Text = "-";
                GameNameText.Text = "-";
                StatusText.Text = "-";
                PortText.Text = "-";
                UptimeText.Text = "-";
                CpuText.Text = "0%";
                RamText.Text = "0 MB";
                CpuProgressBar.Value = 0;
                RamProgressBar.Value = 0;
                return;
            }

            ServerNameText.Text = _server.Name;
            GameNameText.Text = _server.GameName;
            StatusText.Text = _server.Status.ToString();
            StatusText.Foreground = _server.Status == ServerStatus.Running ? Brushes.Green : 
                                   _server.Status == ServerStatus.Stopped ? Brushes.Red : Brushes.Orange;
            PortText.Text = _server.Port.ToString();
            
            if (_server.Status == ServerStatus.Running)
            {
                var uptime = DateTime.Now - _serverStartTime;
                UptimeText.Text = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            }
            else
            {
                UptimeText.Text = "Not running";
            }
        }

        private void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null && System.IO.Directory.Exists(_server.InstallPath))
            {
                Process.Start("explorer.exe", _server.InstallPath);
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null && System.IO.Directory.Exists(_server.InstallPath))
            {
                Process.Start("explorer.exe", _server.InstallPath);
            }
        }

        private void Connect_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null && _server.Status == ServerStatus.Running)
            {
                var connectString = $"steam://connect/localhost:{_server.Port}";
                Process.Start(new ProcessStartInfo(connectString) { UseShellExecute = true });
            }
        }

        private async void Firewall_Click(object sender, RoutedEventArgs e)
        {
            if (_server == null) return;

            var ruleName = $"PulsePanel - {_server.Name}";
            
            try
            {
                // Check for port conflicts first
                var isPortInUse = await FirewallManager.IsPortInUse(_server.Port);
                if (isPortInUse && _server.Status != ServerStatus.Running)
                {
                    OutputReceived?.Invoke($"WARNING: Port {_server.Port} is already in use by another application");
                }
                
                OutputReceived?.Invoke($"Opening firewall port {_server.Port} for {_server.Name}...");
                
                // Open both UDP and TCP
                var udpSuccess = await FirewallManager.OpenFirewallPort(_server.Port, ruleName, true);
                var tcpSuccess = await FirewallManager.OpenFirewallPort(_server.Port, ruleName, false);
                
                if (udpSuccess && tcpSuccess)
                {
                    OutputReceived?.Invoke($"Firewall rules created successfully for port {_server.Port} (UDP & TCP)");
                    new ToastNotification("Firewall Updated", $"Port {_server.Port} opened successfully");
                }
                else
                {
                    OutputReceived?.Invoke("Failed to create firewall rules. Run as administrator.");
                }
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Firewall error: {ex.Message}");
            }
        }

        private void UpdateUptime(object? sender, EventArgs e)
        {
            if (_server?.Status == ServerStatus.Running)
            {
                var uptime = DateTime.Now - _serverStartTime;
                UptimeText.Text = $"{uptime.Hours:D2}:{uptime.Minutes:D2}:{uptime.Seconds:D2}";
            }
        }

        private void UpdateDiskUsage(object? sender, EventArgs? e)
        {
            if (_server == null) return;
            
            try
            {
                var drive = new DriveInfo(Path.GetPathRoot(_server.InstallPath) ?? "C:\\");
                var usedPercent = (1.0 - (double)drive.AvailableFreeSpace / drive.TotalSize) * 100;
                DiskProgressBar.Value = Math.Min(usedPercent, 100);
                DiskText.Text = $"{usedPercent:F1}%";
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Failed to get disk usage: {ex.Message}");
                DiskText.Text = "N/A";
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _uptimeTimer?.Stop();
                _diskTimer?.Stop();
                _processMonitor?.Dispose();
                _disposed = true;
            }
        }
    }
}