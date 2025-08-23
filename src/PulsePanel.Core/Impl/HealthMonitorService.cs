using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using PulsePanel.Core.Services;

namespace PulsePanel.Core.Impl
{
    public sealed class HealthMonitorService : IHealthMonitorService
    {
        private readonly IServerProcessService _proc;
        private readonly IProvenance _prov;
        private TimeSpan _interval = TimeSpan.FromSeconds(5);

        public event EventHandler<ServerHealthSnapshot>? HealthUpdated;

        public HealthMonitorService(IServerProcessService proc, IProvenance prov)
        {
            _proc = proc;
            _prov = prov;
        }

        public void SetInterval(TimeSpan interval) => _interval = interval;

        public async Task StartAsync(CancellationToken token)
        {
            var cid = Guid.NewGuid().ToString("n");
            _prov.Info("HealthMonitor.Start", new { Interval = _interval }, cid);

            while (!token.IsCancellationRequested)
            {
                try
                {
                    var running = _proc.ListRunningServers(); // implement in your service
                    foreach (var srv in running)
                    {
                        var snap = GetSnapshot(srv);
                        HealthUpdated?.Invoke(this, snap);
                        _prov.Info("HealthMonitor.Snapshot", snap, cid);
                    }
                }
                catch (Exception ex)
                {
                    _prov.Error("HealthMonitor.Error", ex.Message, null, cid);
                }

                await Task.Delay(_interval, token);
            }

            _prov.Info("HealthMonitor.Stop", new { }, cid);
        }

        private ServerHealthSnapshot GetSnapshot(ServerProcessInfo info)
        {
            var proc = Process.GetProcessById(info.ProcessId);
            double cpu = CpuHelper.GetCpuUsage(proc); // implement helper
            double mem = proc.WorkingSet64 / 1024d / 1024d;
            var uptime = DateTime.Now - proc.StartTime;

            return new ServerHealthSnapshot
            {
                ServerId = info.ServerId,
                Timestamp = DateTime.UtcNow,
                CpuPercent = cpu,
                MemoryMb = mem,
                Uptime = uptime
            };
        }
    }
}
