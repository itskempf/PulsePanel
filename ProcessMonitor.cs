using System.Diagnostics;

namespace PulsePanel
{
    public class ProcessMonitor : IDisposable
    {
        private readonly Timer _monitorTimer;
        private GameServer? _server;
        private bool _wasRunning;
        private bool _disposed = false;

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
            if (_disposed || _server?.ServerProcess == null) return;

            try
            {
                var process = _server.ServerProcess;
                
                if (process.HasExited)
                {
                    if (_wasRunning && _server.Status == ServerStatus.Running)
                    {
                        _server.Status = ServerStatus.Crashed;
                        ProcessCrashed?.Invoke(_server);
                        Logger.LogWarning($"Server process crashed: {_server.Name}");
                    }
                    _wasRunning = false;
                    return;
                }

                // Get CPU and RAM usage with caching
                var cacheKey = $"resources_{_server.Id}";
                var cachedData = PerformanceCache.Get<ResourceData>(cacheKey);
                
                if (cachedData == null)
                {
                    var cpuPercent = GetCpuUsage(process);
                    var ramBytes = process.WorkingSet64;
                    
                    cachedData = new ResourceData(cpuPercent, ramBytes);
                    PerformanceCache.Set(cacheKey, cachedData, TimeSpan.FromSeconds(2));
                }
                
                ResourcesUpdated?.Invoke(_server, cachedData.CpuPercent, cachedData.RamBytes);
                _wasRunning = true;
            }
            catch (Exception ex)
            {
                Logger.LogWarning($"Process monitoring error for {_server?.Name}: {ex.Message}");
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
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _monitorTimer?.Dispose();
                _disposed = true;
            }
        }
        
        private class ResourceData
        {
            public float CpuPercent { get; }
            public long RamBytes { get; }
            
            public ResourceData(float cpuPercent, long ramBytes)
            {
                CpuPercent = cpuPercent;
                RamBytes = ramBytes;
            }
        }
    }
}