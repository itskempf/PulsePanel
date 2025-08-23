using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

public class ServerProcessService : IServerProcessService
{
    private readonly Dictionary<string, Process> _running = new();
    private readonly ProvenanceLogger _logger;
    private readonly IServerStore _serverStore;

    public ServerProcessService(ProvenanceLogger logger, IServerStore serverStore)
    {
        _logger = logger;
        _serverStore = serverStore;
    }

    public void StartServer(string id, string exePath, string args)
    {
        if (_running.ContainsKey(id))
            throw new InvalidOperationException($"Server '{id}' is already running");

        if (!File.Exists(exePath))
            throw new FileNotFoundException($"Executable not found: {exePath}");

        var proc = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exePath,
                Arguments = args,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

        proc.OutputDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _logger.Log(new LogEntry
                {
                    Action = "ServerOutput",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { { "ServerId", id }, { "Line", e.Data } }
                });
            }
        };

        proc.ErrorDataReceived += (s, e) =>
        {
            if (!string.IsNullOrWhiteSpace(e.Data))
            {
                _logger.Log(new LogEntry
                {
                    Action = "ServerError",
                    Timestamp = DateTime.UtcNow,
                    Metadata = new Dictionary<string, object> { { "ServerId", id }, { "Line", e.Data } }
                });
            }
        };

        proc.Exited += (s, e) =>
        {
            _running.Remove(id);
            _serverStore.UpdateStatus(id, ServerStatus.Stopped);

            _logger.Log(new LogEntry
            {
                Action = "ServerExited",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { { "ServerId", id }, { "ExitCode", proc.ExitCode } }
            });
        };

        proc.Start();
        proc.BeginOutputReadLine();
        proc.BeginErrorReadLine();

        _running[id] = proc;
        _serverStore.UpdateStatus(id, ServerStatus.Running);

        _logger.Log(new LogEntry
        {
            Action = "ServerStarted",
            Timestamp = DateTime.UtcNow,
            Metadata = new Dictionary<string, object> { { "ServerId", id }, { "Executable", exePath }, { "Arguments", args } }
        });
    }

    public void StopServer(string id)
    {
        if (_running.TryGetValue(id, out var proc))
        {
            proc.Kill(true);
            _running.Remove(id);
            _serverStore.UpdateStatus(id, ServerStatus.Stopped);

            _logger.Log(new LogEntry
            {
                Action = "ServerStopped",
                Timestamp = DateTime.UtcNow,
                Metadata = new Dictionary<string, object> { { "ServerId", id } }
            });
        }
        else
        {
            throw new InvalidOperationException($"Server '{id}' is not running");
        }
    }

    public bool IsServerRunning(string id) => _running.ContainsKey(id);
}