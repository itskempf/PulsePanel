using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using PulsePanel.Core.Events;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public class ServerProcessService : IServerProcessService
    {
        private readonly Dictionary<string, Process> _running = new();
        private readonly IProvenanceLogger _logger;
        private readonly IServerStore _serverStore;
        private readonly IEventBus _eventBus;

        public ServerProcessService(IProvenanceLogger logger, IServerStore serverStore, IEventBus eventBus)
        {
            _logger = logger;
            _serverStore = serverStore;
            _eventBus = eventBus;
        }

        public void StartServer(string id, string exePath, string args)
        {
            var correlationId = Guid.NewGuid().ToString("n");

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
                    _logger.Log(new ProvenanceEvent
                    {
                        Action = "Server.Output",
                        Category = "Server",
                        ResourceId = id,
                        CorrelationId = correlationId,
                        Timestamp = DateTime.UtcNow,
                        Metadata = new { Line = e.Data }
                    });
                }
            };

            proc.ErrorDataReceived += (s, e) =>
            {
                if (!string.IsNullOrWhiteSpace(e.Data))
                {
                    _logger.Log(new ProvenanceEvent
                    {
                        Action = "Server.Error",
                        Category = "Server",
                        ResourceId = id,
                        CorrelationId = correlationId,
                        Timestamp = DateTime.UtcNow,
                        Metadata = new { Line = e.Data }
                    });
                }
            };

            proc.Exited += (s, e) =>
            {
                _running.Remove(id);
                _serverStore.UpdateStatus(id, ServerStatus.Stopped);

                var exitEvent = proc.ExitCode == 0 
                    ? new ServerStoppedEvent(id, correlationId)
                    : new ServerCrashedEvent(id, $"Process exited with code {proc.ExitCode}", correlationId);
                
                _eventBus.Publish(exitEvent);
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            _running[id] = proc;
            _serverStore.UpdateStatus(id, ServerStatus.Running);

            _eventBus.Publish(new ServerStartedEvent(id, correlationId));
        }

        public void StopServer(string id)
        {
            if (_running.TryGetValue(id, out var proc))
            {
                var correlationId = Guid.NewGuid().ToString("n");
                
                proc.Kill(true);
                _running.Remove(id);
                _serverStore.UpdateStatus(id, ServerStatus.Stopped);

                _eventBus.Publish(new ServerStoppedEvent(id, correlationId));
            }
            else
            {
                throw new InvalidOperationException($"Server '{id}' is not running");
            }
        }

        public bool IsServerRunning(string id) => _running.ContainsKey(id);
    }
}