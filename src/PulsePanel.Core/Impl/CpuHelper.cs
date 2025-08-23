using System;
using System.Diagnostics;

namespace PulsePanel.Core.Impl
{
    public static class CpuHelper
    {
        public static double GetCpuUsage(Process process)
        {
            try
            {
                var startCpu = process.TotalProcessorTime;
                var startTime = DateTime.UtcNow;
                System.Threading.Thread.Sleep(100);
                var endCpu = process.TotalProcessorTime;
                var endTime = DateTime.UtcNow;

                var cpuUsedMs = (endCpu - startCpu).TotalMilliseconds;
                var totalMsPassed = (endTime - startTime).TotalMilliseconds;
                var cpuUsageTotal = cpuUsedMs / (Environment.ProcessorCount * totalMsPassed);
                return Math.Round(cpuUsageTotal * 100, 1);
            }
            catch
            {
                return 0;
            }
        }
    }
}
