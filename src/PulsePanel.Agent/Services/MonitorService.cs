using Grpc.Core;
using PulsePanel.Contracts;
using System;

namespace PulsePanel.Agent.Services
{
    public class MonitorService : PulsePanel.Contracts.Monitor.MonitorBase
    {
        public override Task<HealthResponse> GetHealth(HealthRequest request, ServerCallContext context)
        {
            var resp = new HealthResponse();
            resp.Items.Add(new Vitals
            {
                ServerId = "build-server",
                Status = "Running",
                CpuPercent = 5.2,
                RamPercent = 26.3,
                DiskPercent = 71.0,
                NetInMbps = 1.2,
                NetOutMbps = 0.8,
                Pid = 1234,
                Port = 25565,
                HeartbeatAgeSec = 1.1,
                ServiceStatus = "Running",
                UptimeSec = 8280,
                UpdatedAtUtc = DateTime.UtcNow.ToString("o")
            });
            return Task.FromResult(resp);
        }
    }
}
