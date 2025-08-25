
using System;

namespace PulsePanel.Core.Services
{
    public interface IHealthMonitoringService : IDisposable
    {
        void Start();
        void Stop();
    }
}
