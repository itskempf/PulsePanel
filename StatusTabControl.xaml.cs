using System.Diagnostics;
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
                try
                {
                    var ramMB = ramBytes / (1024 * 1024);
                    
                    if (server.Status == ServerStatus.Running)
                    {
                        var uptime = DateTime.Now - _serverStartTime;
                        OutputReceived?.Invoke($"Server {server.Name}: CPU {cpuPercent:F1}%, RAM {ramMB} MB, Uptime {uptime.Hours}h {uptime.Minutes}m");
                    }
                }
                catch { }
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
                OutputReceived?.Invoke("No server selected");
                return;
            }

            OutputReceived?.Invoke($"Server: {_server.Name} ({_server.GameName}) - Status: {_server.Status} - Port: {_server.Port}");
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
                }
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Firewall error: {ex.Message}");
            }
        }
    }
}