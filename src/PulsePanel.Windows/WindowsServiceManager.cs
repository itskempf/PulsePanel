using System;
using System.Diagnostics;
using System.ServiceProcess;
using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Services;

namespace PulsePanel.Windows
{
    public class WindowsServiceManager : IWindowsServiceManager
    {
        private readonly IProvenanceLogger _logger;

        public WindowsServiceManager(IProvenanceLogger logger)
        {
            _logger = logger;
        }

        public void InstallService(string exePath, string serviceName)
        {
            RunScCommand($"create \"{serviceName}\" binPath= \"{exePath}\" start= auto");

            _logger.Log(new LogEntry
            {
                Action = "WindowsServiceInstalled",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "ServiceName", serviceName },
                    { "Executable", exePath }
                }
            });
        }

        public void StartService(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status != ServiceControllerStatus.Running)
                sc.Start();

            _logger.Log(new LogEntry
            {
                Action = "WindowsServiceStarted",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "ServiceName", serviceName }
                }
            });
        }

        public void StopService(string serviceName)
        {
            using var sc = new ServiceController(serviceName);
            if (sc.Status != ServiceControllerStatus.Stopped)
                sc.Stop();

            _logger.Log(new LogEntry
            {
                Action = "WindowsServiceStopped",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "ServiceName", serviceName }
                }
            });
        }

        public void RemoveService(string serviceName)
        {
            RunScCommand($"delete \"{serviceName}\"");

            _logger.Log(new LogEntry
            {
                Action = "WindowsServiceRemoved",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "ServiceName", serviceName }
                }
            });
        }

        private void RunScCommand(string arguments)
        {
            var proc = Process.Start(new ProcessStartInfo
            {
                FileName = "sc.exe",
                Arguments = arguments,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            });

            proc.WaitForExit();
        }
    }
}
