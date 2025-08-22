using System.Collections.Concurrent; // Added
using System.Diagnostics;
using System.Text;
using PulsePanel.Core.Services;
using System;
using System.IO;
using System.Threading.Tasks; // Added for async methods
using System.Collections.Generic; // Added for Dictionary
using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Models; // For ServerEntry and ServerStatus

namespace PulsePanel.Core.Services;

public class ServerProcessService {
    private readonly ProvenanceLogger _logger; // Changed to ProvenanceLogger
    private readonly ConcurrentDictionary<string, Process> _runningServers = new(); // Added
    public ServerProcessService(ProvenanceLogger logger) { // Changed to ProvenanceLogger
        _logger = logger;
    }

    private string ServersRoot => Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "servers"));

    public string GetServerDir(string id) => Path.Combine(ServersRoot, id);
    public string FilesDir(string id) => Path.Combine(GetServerDir(id), "files");
    public string ConfigDir(string id) => Path.Combine(GetServerDir(id), "config");
    public string LogsDir(string id) => Path.Combine(GetServerDir(id), "logs");

    public void EnsureLayout(string id) {
        Directory.CreateDirectory(FilesDir(id));
        Directory.CreateDirectory(ConfigDir(id));
        Directory.CreateDirectory(LogsDir(id));
    }

/*
    public async Task<(Process? proc, Exception? error)> StartServer(ServerEntry serverEntry)
    {
        try
        {
            EnsureLayout(serverEntry.Id);
            var work = FilesDir(serverEntry.Id);
            var outLogPath = Path.Combine(LogsDir(serverEntry.Id), "server.out.log");
            var errLogPath = Path.Combine(LogsDir(serverEntry.Id), "server.err.log");
            var outLog = new StreamWriter(new FileStream(outLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
            var errLog = new StreamWriter(new FileStream(errLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };

            // Placeholder: In a real scenario, Exec and Args would come from GameDefinition or ServerEntry config
            // For now, assuming serverEntry has these or they are looked up.
            // For demonstration, let's use dummy values or assume they are part of ServerEntry for now.
            // If ServerEntry doesn't have them, we'd need a GameDefinition lookup service.
            string exec = "java"; // Dummy executable
            string args = "-jar server.jar nogui"; // Dummy arguments

            // Attempt to resolve executable path if it's relative
            exec = ResolveExec(work, exec);

            var psi = new ProcessStartInfo
            {
                FileName = exec,
                Arguments = args,
                WorkingDirectory = work,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.OutputDataReceived += (_, e) => { if (e.Data != null) outLog.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) errLog.WriteLine(e.Data); };
            p.Exited += async (_, __) =>
            {
                try
                {
                    outLog.Dispose();
                    errLog.Dispose();
                    _runningServers.TryRemove(serverEntry.Id, out _); // Remove from tracking
                    await _logger.LogAsync(new LogEntry
                    {
                        Action = "Server_Exited",
                        EntityType = "Server",
                        EntityIdentifier = serverEntry.Id,
                        Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exitCode", p.ExitCode } }
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error during server exit cleanup for {serverEntry.Id}: {ex.Message}");
                }
            };

            if (!p.Start())
            {
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Start_Failed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Process failed to start" } }
                });
                return (null, new Exception("Failed to start process."));
            }

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            _runningServers.TryAdd(serverEntry.Id, p); // Add to tracking

            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Started",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "pid", p.Id } }
            });

            return (p, null);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Start_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return (null, ex);
        }
    }

    public async Task<bool> StopServer(ServerEntry serverEntry)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            // Server not tracked as running by this service instance
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Attempt_Not_Running",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
            });
            return false;
        }

        try
        {
            if (p.HasExited)
            {
                _runningServers.TryRemove(serverEntry.Id, out _);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Already_Stopped",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
                });
                return true;
            }

            // Attempt graceful shutdown
            p.CloseMainWindow();
            if (!p.WaitForExit(5000)) // Wait up to 5 seconds for graceful exit
            {
                // Force kill if not exited gracefully
                p.Kill(true);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Force_Killed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
                });
            }

            _runningServers.TryRemove(serverEntry.Id, out _);
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stopped",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<(Process? proc, Exception? error)> RestartServer(ServerEntry serverEntry)
    {
        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restart_Attempt",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        var stopSuccess = await StopServer(serverEntry);
        if (!stopSuccess)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Stop",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Failed to stop server for restart" } }
            });
            return (null, new Exception("Failed to stop server for restart."));
        }

        // Give a small delay before starting again
        await Task.Delay(1000);

        var (proc, error) = await StartServer(serverEntry);
        if (proc == null)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Start",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", error?.Message ?? "Unknown start error" } }
            });
            return (null, error);
        }

        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restarted",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        return (proc, null);
    }

    public ServerStatus GetServerStatus(ServerEntry serverEntry)
    {
        if (_runningServers.ContainsKey(serverEntry.Id))
        {
            var p = _runningServers[serverEntry.Id];
            if (!p.HasExited)
            {
                return ServerStatus.Running;
            }
            else
            {
                _runningServers.TryRemove(serverEntry.Id, out _); // Clean up if process exited
                return ServerStatus.Stopped;
            }
        }
        return ServerStatus.Stopped;
    }

    public async Task<bool> SetCpuPriority(ServerEntry serverEntry, ProcessPriorityClass priority)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.PriorityClass = priority;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<bool> SetCpuAffinity(ServerEntry serverEntry, long affinityMask)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.ProcessorAffinity = (IntPtr)affinityMask;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "affinityHex", affinityMask.ToString("X") } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "exception", ex.Message } }
            });
            return false;
        }
    }
*/

    private static string ResolveExec(string workingDir, string relativeExec) {
        var candidate = Path.Combine(workingDir, relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }
}

/*
    public async Task<(Process? proc, Exception? error)> StartServer(ServerEntry serverEntry)
    {
        try
        {
            EnsureLayout(serverEntry.Id);
            var work = FilesDir(serverEntry.Id);
            var outLogPath = Path.Combine(LogsDir(serverEntry.Id), "server.out.log");
            var errLogPath = Path.Combine(LogsDir(serverEntry.Id), "server.err.log");
            var outLog = new StreamWriter(new FileStream(outLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
            var errLog = new StreamWriter(new FileStream(errLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };

            // Placeholder: In a real scenario, Exec and Args would come from GameDefinition or ServerEntry config
            // For now, assuming serverEntry has these or they are looked up.
            // For demonstration, let's use dummy values or assume they are part of ServerEntry for now.
            // If ServerEntry doesn't have them, we'd need a GameDefinition lookup service.
            string exec = "java"; // Dummy executable
            string args = "-jar server.jar nogui"; // Dummy arguments

            // Attempt to resolve executable path if it's relative
            exec = ResolveExec(work, exec);

            var psi = new ProcessStartInfo
            {
                FileName = exec,
                Arguments = args,
                WorkingDirectory = work,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.OutputDataReceived += (_, e) => { if (e.Data != null) outLog.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) errLog.WriteLine(e.Data); };
            p.Exited += async (_, __) =>
            {
                try
                {
                    outLog.Dispose();
                    errLog.Dispose();
                    _runningServers.TryRemove(serverEntry.Id, out _); // Remove from tracking
                    await _logger.LogAsync(new LogEntry
                    {
                        Action = "Server_Exited",
                        EntityType = "Server",
                        EntityIdentifier = serverEntry.Id,
                        Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exitCode", p.ExitCode } }
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error during server exit cleanup for {serverEntry.Id}: {ex.Message}");
                }
            };

            if (!p.Start())
            {
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Start_Failed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Process failed to start" } }
                });
                return (null, new Exception("Failed to start process."));
            }

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            _runningServers.TryAdd(serverEntry.Id, p); // Add to tracking

            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Started",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "pid", p.Id } }
            });

            return (p, null);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Start_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return (null, ex);
        }
    }


    public async Task<bool> StopServer(ServerEntry serverEntry)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            // Server not tracked as running by this service instance
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Attempt_Not_Running",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
            });
            return false;
        }

        try
        {
            if (p.HasExited)
            {
                _runningServers.TryRemove(serverEntry.Id, out _);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Already_Stopped",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
                });
                return true;
            }

            // Attempt graceful shutdown
            p.CloseMainWindow();
            if (!p.WaitForExit(5000)) // Wait up to 5 seconds for graceful exit
            {
                // Force kill if not exited gracefully
                p.Kill(true);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Force_Killed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
                });
            }

            _runningServers.TryRemove(serverEntry.Id, out _);
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stopped",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<(Process? proc, Exception? error)> RestartServer(ServerEntry serverEntry)
    {
        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restart_Attempt",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        var stopSuccess = await StopServer(serverEntry);
        if (!stopSuccess)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Stop",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Failed to stop server for restart" } }
            });
            return (null, new Exception("Failed to stop server for restart."));
        }

        // Give a small delay before starting again
        await Task.Delay(1000);

        var (proc, error) = await StartServer(serverEntry);
        if (proc == null)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Start",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", error?.Message ?? "Unknown start error" } }
            });
            return (null, error);
        }

        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restarted",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        return (proc, null);
    }

    public ServerStatus GetServerStatus(ServerEntry serverEntry)
    {
        if (_runningServers.ContainsKey(serverEntry.Id))
        {
            var p = _runningServers[serverEntry.Id];
            if (!p.HasExited)
            {
                return ServerStatus.Running;
            }
            else
            {
                _runningServers.TryRemove(serverEntry.Id, out _); // Clean up if process exited
                return ServerStatus.Stopped;
            }
        }
        return ServerStatus.Stopped;
    }

    public async Task<bool> SetCpuPriority(ServerEntry serverEntry, ProcessPriorityClass priority)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.PriorityClass = priority;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<bool> SetCpuAffinity(ServerEntry serverEntry, long affinityMask)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.ProcessorAffinity = (IntPtr)affinityMask;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "affinityHex", affinityMask.ToString("X") } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "exception", ex.Message } }
            });
            return false;
        }
    }
*/

    private static string ResolveExec(string workingDir, string relativeExec) {
        var candidate = Path.Combine(workingDir, relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }
}

    /*
    /*
    public async Task<(Process? proc, Exception? error)> StartServer(ServerEntry serverEntry)
    {
        try
        {
            EnsureLayout(serverEntry.Id);
            var work = FilesDir(serverEntry.Id);
            var outLogPath = Path.Combine(LogsDir(serverEntry.Id), "server.out.log");
            var errLogPath = Path.Combine(LogsDir(serverEntry.Id), "server.err.log");
            var outLog = new StreamWriter(new FileStream(outLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };
            var errLog = new StreamWriter(new FileStream(errLogPath, FileMode.Append, FileAccess.Write, FileShare.Read)) { AutoFlush = true };

            // Placeholder: In a real scenario, Exec and Args would come from GameDefinition or ServerEntry config
            // For now, assuming serverEntry has these or they are looked up.
            // For demonstration, let's use dummy values or assume they are part of ServerEntry for now.
            // If ServerEntry doesn't have them, we'd need a GameDefinition lookup service.
            string exec = "java"; // Dummy executable
            string args = "-jar server.jar nogui"; // Dummy arguments

            // Attempt to resolve executable path if it's relative
            exec = ResolveExec(work, exec);

            var psi = new ProcessStartInfo
            {
                FileName = exec,
                Arguments = args,
                WorkingDirectory = work,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            var p = new Process { StartInfo = psi, EnableRaisingEvents = true };
            p.OutputDataReceived += (_, e) => { if (e.Data != null) outLog.WriteLine(e.Data); };
            p.ErrorDataReceived += (_, e) => { if (e.Data != null) errLog.WriteLine(e.Data); };
            p.Exited += async (_, __) =>
            {
                try
                {
                    outLog.Dispose();
                    errLog.Dispose();
                    _runningServers.TryRemove(serverEntry.Id, out _); // Remove from tracking
                    await _logger.LogAsync(new LogEntry
                    {
                        Action = "Server_Exited",
                        EntityType = "Server",
                        EntityIdentifier = serverEntry.Id,
                        Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exitCode", p.ExitCode } }
                    });
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error during server exit cleanup for {serverEntry.Id}: {ex.Message}");
                }
            };

            if (!p.Start())
            {
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Start_Failed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Process failed to start" } }
                });
                return (null, new Exception("Failed to start process."));
            }

            p.BeginOutputReadLine();
            p.BeginErrorReadLine();

            _runningServers.TryAdd(serverEntry.Id, p); // Add to tracking

            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Started",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "pid", p.Id } }
            });

            return (p, null);
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Start_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return (null, ex);
        }
    }
*/

/*
    public async Task<bool> StopServer(ServerEntry serverEntry)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            // Server not tracked as running by this service instance
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Attempt_Not_Running",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
            });
            return false;
        }

        try
        {
            if (p.HasExited)
            {
                _runningServers.TryRemove(serverEntry.Id, out _);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Already_Stopped",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
                });
                return true;
            }

            // Attempt graceful shutdown
            p.CloseMainWindow();
            if (!p.WaitForExit(5000)) // Wait up to 5 seconds for graceful exit
            {
                // Force kill if not exited gracefully
                p.Kill(true);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Force_Killed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
                });
            }

            _runningServers.TryRemove(serverEntry.Id, out _);
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stopped",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<(Process? proc, Exception? error)> RestartServer(ServerEntry serverEntry)
    {
        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restart_Attempt",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        var stopSuccess = await StopServer(serverEntry);
        if (!stopSuccess)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Stop",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Failed to stop server for restart" } }
            });
            return (null, new Exception("Failed to stop server for restart."));
        }

        // Give a small delay before starting again
        await Task.Delay(1000);

        var (proc, error) = await StartServer(serverEntry);
        if (proc == null)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Start",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", error?.Message ?? "Unknown start error" } }
            });
            return (null, error);
        }

        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restarted",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        return (proc, null);
    }

    public ServerStatus GetServerStatus(ServerEntry serverEntry)
    {
        if (_runningServers.ContainsKey(serverEntry.Id))
        {
            var p = _runningServers[serverEntry.Id];
            if (!p.HasExited)
            {
                return ServerStatus.Running;
            }
            else
            {
                _runningServers.TryRemove(serverEntry.Id, out _); // Clean up if process exited
                return ServerStatus.Stopped;
            }
        }
        return ServerStatus.Stopped;
    }

    public async Task<bool> SetCpuPriority(ServerEntry serverEntry, ProcessPriorityClass priority)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.PriorityClass = priority;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<bool> SetCpuAffinity(ServerEntry serverEntry, long affinityMask)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.ProcessorAffinity = (IntPtr)affinityMask;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "affinityHex", affinityMask.ToString("X") } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "exception", ex.Message } }
            });
            return false;
        }
    }
*/

    private static string ResolveExec(string workingDir, string relativeExec) {
        var candidate = Path.Combine(workingDir, relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }
}

*/

/*
    public async Task<bool> StopServer(ServerEntry serverEntry)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            // Server not tracked as running by this service instance
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Attempt_Not_Running",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
            });
            return false;
        }

        try
        {
            if (p.HasExited)
            {
                _runningServers.TryRemove(serverEntry.Id, out _);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Already_Stopped",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
                });
                return true;
            }

            // Attempt graceful shutdown
            p.CloseMainWindow();
            if (!p.WaitForExit(5000)) // Wait up to 5 seconds for graceful exit
            {
                // Force kill if not exited gracefully
                p.Kill(true);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Force_Killed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
                });
            }

            _runningServers.TryRemove(serverEntry.Id, out _);
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stopped",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<(Process? proc, Exception? error)> RestartServer(ServerEntry serverEntry)
    {
        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restart_Attempt",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        var stopSuccess = await StopServer(serverEntry);
        if (!stopSuccess)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Stop",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Failed to stop server for restart" } }
            });
            return (null, new Exception("Failed to stop server for restart."));
        }

        // Give a small delay before starting again
        await Task.Delay(1000);

        var (proc, error) = await StartServer(serverEntry);
        if (proc == null)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Start",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", error?.Message ?? "Unknown start error" } }
            });
            return (null, error);
        }

        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restarted",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        return (proc, null);
    }

    public ServerStatus GetServerStatus(ServerEntry serverEntry)
    {
        if (_runningServers.ContainsKey(serverEntry.Id))
        {
            var p = _runningServers[serverEntry.Id];
            if (!p.HasExited)
            {
                return ServerStatus.Running;
            }
            else
            {
                _runningServers.TryRemove(serverEntry.Id, out _); // Clean up if process exited
                return ServerStatus.Stopped;
            }
        }
        return ServerStatus.Stopped;
    }

    public async Task<bool> SetCpuPriority(ServerEntry serverEntry, ProcessPriorityClass priority)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.PriorityClass = priority;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<bool> SetCpuAffinity(ServerEntry serverEntry, long affinityMask)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.ProcessorAffinity = (IntPtr)affinityMask;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "affinityHex", affinityMask.ToString("X") } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "exception", ex.Message } }
            });
            return false;
        }
    }
*/

    private static string ResolveExec(string workingDir, string relativeExec) {
        var candidate = Path.Combine(workingDir, relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }
}

    /*
    public async Task<bool> StopServer(ServerEntry serverEntry)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            // Server not tracked as running by this service instance
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Attempt_Not_Running",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not tracked as running" } }
            });
            return false;
        }

        try
        {
            if (p.HasExited)
            {
                _runningServers.TryRemove(serverEntry.Id, out _);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Already_Stopped",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
                });
                return true;
            }

            // Attempt graceful shutdown
            p.CloseMainWindow();
            if (!p.WaitForExit(5000)) // Wait up to 5 seconds for graceful exit
            {
                // Force kill if not exited gracefully
                p.Kill(true);
                await _logger.LogAsync(new LogEntry
                {
                    Action = "Server_Force_Killed",
                    EntityType = "Server",
                    EntityIdentifier = serverEntry.Id,
                    Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
                });
            }

            _runningServers.TryRemove(serverEntry.Id, out _);
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stopped",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Stop_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<(Process? proc, Exception? error)> RestartServer(ServerEntry serverEntry)
    {
        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restart_Attempt",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        var stopSuccess = await StopServer(serverEntry);
        if (!stopSuccess)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Stop",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Failed to stop server for restart" } }
            });
            return (null, new Exception("Failed to stop server for restart."));
        }

        // Give a small delay before starting again
        await Task.Delay(1000);

        var (proc, error) = await StartServer(serverEntry);
        if (proc == null)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_Restart_Failed_Start",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", error?.Message ?? "Unknown start error" } }
            });
            return (null, error);
        }

        await _logger.LogAsync(new LogEntry
        {
            Action = "Server_Restarted",
            EntityType = "Server",
            EntityIdentifier = serverEntry.Id,
            Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name } }
        });

        return (proc, null);
    }

    public ServerStatus GetServerStatus(ServerEntry serverEntry)
    {
        if (_runningServers.ContainsKey(serverEntry.Id))
        {
            var p = _runningServers[serverEntry.Id];
            if (!p.HasExited)
            {
                return ServerStatus.Running;
            }
            else
            {
                _runningServers.TryRemove(serverEntry.Id, out _); // Clean up if process exited
                return ServerStatus.Stopped;
            }
        }
        return ServerStatus.Stopped;
    }

    public async Task<bool> SetCpuPriority(ServerEntry serverEntry, ProcessPriorityClass priority)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.PriorityClass = priority;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuPriority_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "priority", priority.ToString() }, { "exception", ex.Message } }
            });
            return false;
        }
    }

    public async Task<bool> SetCpuAffinity(ServerEntry serverEntry, long affinityMask)
    {
        if (!_runningServers.ContainsKey(serverEntry.Id))
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Failed",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "reason", "Server not running" } }
            });
            return false;
        }
        var p = _runningServers[serverEntry.Id];

        try
        {
            p.ProcessorAffinity = (IntPtr)affinityMask;
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "affinityHex", affinityMask.ToString("X") } }
            });
            return true;
        }
        catch (Exception ex)
        {
            await _logger.LogAsync(new LogEntry
            {
                Action = "Server_SetCpuAffinity_Exception",
                EntityType = "Server",
                EntityIdentifier = serverEntry.Id,
                Metadata = new Dictionary<string, object> { { "serverName", serverEntry.Name }, { "affinityMask", affinityMask }, { "exception", ex.Message } }
            });
            return false;
        }
    }
*/

    private static string ResolveExec(string workingDir, string relativeExec) {
        var candidate = Path.Combine(workingDir, relativeExec.Replace("/", Path.DirectorySeparatorChar.ToString()));
        if (OperatingSystem.IsWindows() && !candidate.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)) {
            if (File.Exists(candidate + ".exe")) candidate += ".exe";
        }
        return candidate;
    }
}