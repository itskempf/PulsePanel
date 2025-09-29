using System.Diagnostics;
using System.IO;

namespace PulsePanel
{
    public class SteamCmdManager
    {
        private readonly string _steamCmdPath;
        public event Action<string>? OutputReceived;

        public SteamCmdManager(string steamCmdPath = @"C:\steamcmd\steamcmd.exe")
        {
            _steamCmdPath = steamCmdPath;
        }

        public async Task<bool> InstallOrUpdateServerAsync(GameServer server)
        {
            if (!File.Exists(_steamCmdPath))
            {
                OutputReceived?.Invoke("SteamCMD not found. Please install SteamCMD first.");
                return false;
            }

            server.Status = ServerStatus.Updating;
            
            var args = $"+force_install_dir \"{server.InstallPath}\" +login anonymous +app_update {server.AppId} validate +quit";
            
            try
            {
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = _steamCmdPath,
                        Arguments = args,
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (s, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OutputReceived?.Invoke(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                await process.WaitForExitAsync();

                server.Status = ServerStatus.Stopped;
                return process.ExitCode == 0;
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error updating server: {ex.Message}");
                server.Status = ServerStatus.Stopped;
                return false;
            }
        }

        public bool StartServer(GameServer server)
        {
            if (server.Status != ServerStatus.Stopped)
                return false;

            if (!File.Exists(server.ExecutablePath))
            {
                OutputReceived?.Invoke($"Server executable not found: {server.ExecutablePath}");
                return false;
            }

            try
            {
                server.Status = ServerStatus.Starting;
                
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = server.ExecutablePath,
                        Arguments = server.StartupArgs,
                        WorkingDirectory = Path.GetDirectoryName(server.ExecutablePath),
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true
                    }
                };

                process.OutputDataReceived += (s, e) => {
                    if (!string.IsNullOrEmpty(e.Data))
                        OutputReceived?.Invoke(e.Data);
                };

                process.Start();
                process.BeginOutputReadLine();
                
                server.ServerProcess = process;
                server.Status = ServerStatus.Running;
                
                OutputReceived?.Invoke($"Server {server.Name} started successfully.");
                return true;
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error starting server: {ex.Message}");
                server.Status = ServerStatus.Stopped;
                return false;
            }
        }

        public bool StopServer(GameServer server)
        {
            if (server.Status != ServerStatus.Running || server.ServerProcess == null)
                return false;

            try
            {
                server.Status = ServerStatus.Stopping;
                
                if (!server.ServerProcess.HasExited)
                {
                    server.ServerProcess.Kill();
                    server.ServerProcess.WaitForExit(5000);
                }
                
                server.ServerProcess = null;
                server.Status = ServerStatus.Stopped;
                
                OutputReceived?.Invoke($"Server {server.Name} stopped.");
                return true;
            }
            catch (Exception ex)
            {
                OutputReceived?.Invoke($"Error stopping server: {ex.Message}");
                return false;
            }
        }
    }
}