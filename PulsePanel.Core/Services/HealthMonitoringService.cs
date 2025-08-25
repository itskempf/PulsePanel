using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public sealed class HealthMonitoringService : IHealthMonitoringService
    {
        private readonly IServerService _servers;
        private Timer? _timer;
        private DateTime _last = DateTime.UtcNow;
        private readonly System.Collections.Concurrent.ConcurrentDictionary<int, TimeSpan> _cpu = new();
        public HealthMonitoringService(IServerService servers) { _servers = servers; }
        public void Start()
        {
            _timer = new Timer(async _ => await Tick(), null, 2000, 2000);
        }
        public void Stop() { _timer?.Dispose(); _timer = null; }
        public void Dispose() => Stop();

        private async Task Tick()
        {
            var now = DateTime.UtcNow;
            var interval = (now - _last).TotalSeconds;
            _last = now;
            var list = await _servers.GetAllServersAsync();
            foreach (var s in list.Where(s => s.ProcessId.HasValue))
            {
                try
                {
                    var p = Process.GetProcessById(s.ProcessId!.Value);
                    var total = p.TotalProcessorTime;
                    var last = _cpu.AddOrUpdate(p.Id, total, (_, old) => total);
                    var delta = (total - last).TotalSeconds;
                    _cpu[p.Id] = total;
                    s.CpuUsage = Math.Max(0, Math.Min(100, 100.0 * delta / interval / Environment.ProcessorCount));
                    s.RamUsage = p.WorkingSet64 / (1024.0 * 1024.0);
                }
                catch
                {
                    s.ProcessId = null; s.CpuUsage = 0; s.RamUsage = 0; s.Status = Models.ServerStatus.Stopped;
                }
                await _servers.UpdateAsync(s);
            }
        }
    }
}
