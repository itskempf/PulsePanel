using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public interface IHealthMonitorService
    {
        event EventHandler<ServerHealthSnapshot> HealthUpdated;
        Task StartAsync(CancellationToken token);
        void SetInterval(TimeSpan interval);
    }

    public sealed class ServerHealthSnapshot
    {
        public string ServerId { get; set; } = "";
        public DateTime Timestamp { get; set; }
        public double CpuPercent { get; set; }
        public double MemoryMb { get; set; }
        public TimeSpan Uptime { get; set; }
    }
}
