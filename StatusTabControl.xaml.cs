using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace PulsePanel
{
    public partial class StatusTabControl : UserControl
    {
        private GameServer? _server;
        private readonly ProcessMonitor _processMonitor = new();
        private DateTime _serverStartTime;

        public event Action<string>? OutputReceived;

        public StatusTabControl()
        {
            InitializeComponent();
            _processMonitor.ResourcesUpdated += OnResourcesUpdated;
            _processMonitor.ProcessCrashed += OnProcessCrashed;
        }

        public void UpdateServer(GameServer? server)
        {
            if (_server != server)
            {
                _processMonitor.StopMonitoring();
                _server = server;
                
                if (_server?.Status == ServerStatus.Running)
                {
                    _serverStartTime = DateTime.Now;
                    _processMonitor.StartMonitoring(_server);
                }
            }
            
            UpdateDisplay();
        }

        private void OnResourcesUpdated(GameServer server, float cpuPercent, long ramBytes)
        {
            Dispatcher.Invoke(() =>
            {
                CpuProgressBar.Value = Math.Min(cpuPercent, 100);
                CpuText.Text = $"{cpuPercent:F1}%";
                
                var ramMB = ramBytes / (1024 * 1024);
                RamProgressBar.Value = Math.Min(ramMB / 10, 100); // Assume 1GB max for progress
                RamText.Text = $"{ramMB} MB";
                
                // Update uptime
                if (server.Status == ServerStatus.Running)
                {
                    var uptime = DateTime.Now - _serverStartTime;
                    UptimeText.Text = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
                }
            });
        }

        private void OnProcessCrashed(GameServer server)
        {
            Dispatcher.Invoke(() =>
            {
                new ToastNotification("Server Crashed!", $"{server.Name} has stopped unexpectedly", true);
                OutputReceived?.Invoke($"CRASH DETECTED: {server.Name} process terminated unexpectedly");
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
                CpuProgressBar.Value = 0;
                RamProgressBar.Value = 0;
                CpuText.Text = "0%";
                RamText.Text = "0 MB";
                return;
            }

            ServerNameText.Text = _server.Name;
            GameNameText.Text = _server.GameName;
            StatusText.Text = _server.Status.ToString();
            PortText.Text = _server.Port.ToString();
            
            // Color-code status
            StatusText.Foreground = _server.Status switch
            {
                ServerStatus.Running => Brushes.Green,
                ServerStatus.Crashed => Brushes.Red,
                ServerStatus.Stopped => Brushes.Gray,
                _ => Brushes.Orange
            };
            
            if (_server.Status != ServerStatus.Running)
            {
                UptimeText.Text = "Stopped";
                CpuProgressBar.Value = 0;
                RamProgressBar.Value = 0;
                CpuText.Text = "0%";
                RamText.Text = "0 MB";
            }
        }

        private void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null && Directory.Exists(_server.InstallPath))
            {
                var logPath = Path.Combine(_server.InstallPath, "logs");
                if (Directory.Exists(logPath))
                {
                    Process.Start("explorer.exe", logPath);
                }
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (_server != null && Directory.Exists(_server.InstallPath))
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

            FirewallBtn.IsEnabled = false;
            var ruleName = $"PulsePanel - {_server.Name}";
            
            try
            {
                OutputReceived?.Invoke($"Opening firewall port {_server.Port} for {_server.Name}...");
                var success = await FirewallManager.OpenFirewallPort(_server.Port, ruleName);
                
                if (success)
                {
                    OutputReceived?.Invoke($"Firewall rules created successfully for port {_server.Port}");
                    new ToastNotification("Firewall Updated", $"Port {_server.Port} opened successfully");
                }
                else
                {
                    OutputReceived?.Invoke("Failed to create firewall rules. Run as administrator.");
                    MessageBox.Show("Failed to create firewall rules. Please run PulsePanel as administrator.", "Firewall Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            finally
            {
                FirewallBtn.IsEnabled = true;
            }
        }
    }
}