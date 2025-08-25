
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public sealed class ServerProcessService : IServerProcessService
    {
        public event EventHandler<ServerOutputEventArgs>? OutputReceived;

        public async Task StartServerAsync(ServerInstance server)
        {
            string runCmd = Path.Combine(server.InstallPath, "run.cmd");
            string file = File.Exists(runCmd) ? runCmd : FindExecutable(server.InstallPath) ?? throw new FileNotFoundException("No runnable found");

            var psi = new ProcessStartInfo
            {
                FileName = file,
                WorkingDirectory = server.InstallPath,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            var proc = Process.Start(psi) ?? throw new InvalidOperationException("Failed to start process");
            server.ProcessId = proc.Id;
            server.Status = ServerStatus.Running;
            proc.OutputDataReceived += (_, e) => { if (e.Data != null) OutputReceived?.Invoke(this, new ServerOutputEventArgs(server.Id, e.Data, false)); };
            proc.ErrorDataReceived  += (_, e) => { if (e.Data != null) OutputReceived?.Invoke(this, new ServerOutputEventArgs(server.Id, e.Data, true)); };
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            await Task.CompletedTask;
        }

        public async Task StopServerAsync(ServerInstance server)
        {
            if (server.ProcessId is int pid)
            {
                try
                {
                    var proc = Process.GetProcessById(pid);
                    if (!proc.HasExited)
                    {
                        proc.CloseMainWindow();
                        await Task.Delay(1000);
                        if (!proc.HasExited) proc.Kill(true);
                    }
                }
                catch { }
            }
            server.ProcessId = null;
            server.Status = ServerStatus.Stopped;
        }

        private static string? FindExecutable(string path)
        {
            foreach (var name in new[] {"run.cmd", "run.bat"})
            {
                var f = Path.Combine(path, name);
                if (File.Exists(f)) return f;
            }
            var exes = Directory.GetFiles(path, "*.exe");
            if (exes.Length > 0) return exes[0];
            return null;
        }
    }
}
