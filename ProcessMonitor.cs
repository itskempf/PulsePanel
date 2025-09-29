using System.Diagnostics;

namespace PulsePanel
{
    public class ProcessMonitor
    {
        private readonly Timer _monitorTimer;
        private GameServer? _server;
        private bool _wasRunning;

        public event Action<GameServer, float, long>? ResourcesUpdated; // CPU%, RAM bytes
        public event Action<GameServer>? ProcessCrashed;

        public ProcessMonitor()
        {
            _monitorTimer = new Timer(MonitorProcess, null, Timeout.Infinite, Timeout.Infinite);
        }

        public void StartMonitoring(GameServer server)
        {
            _server = server;
            _wasRunning = server.Status == ServerStatus.Running;
            _monitorTimer.Change(1000, 1000); // Check every second
        }

        public void StopMonitoring()
        {
            _monitorTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _server = null;
        }

        private void MonitorProcess(object? state)
        {
            if (_server?.ServerProcess == null) return;

            try
            {
                var process = _server.ServerProcess;
                
                if (process.HasExited)
                {
                    if (_wasRunning && _server.Status == ServerStatus.Running)
                    {
                        _server.Status = ServerStatus.Crashed;
                        ProcessCrashed?.Invoke(_server);
                    }
                    _wasRunning = false;
                    return;
                }

                // Get CPU and RAM usage
                var cpuPercent = GetCpuUsage(process);
                var ramBytes = process.WorkingSet64;
                
                ResourcesUpdated?.Invoke(_server, cpuPercent, ramBytes);
                _wasRunning = true;
            }
            catch
            {
                // Process might be disposed or inaccessible
            }
        }

        private static float GetCpuUsage(Process process)
        {
            try
            {
                return (float)process.TotalProcessorTime.TotalMilliseconds / Environment.ProcessorCount / 10;
            }
            catch
            {
                return 0;
            }
        }

        public void Dispose()
        {
            _monitorTimer?.Dispose();
        }
    }
}