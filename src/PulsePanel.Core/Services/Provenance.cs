using System;
using System.Diagnostics;
using System.Reflection;

namespace PulsePanel.Core.Services
{
    public sealed class Provenance : IProvenance
    {
        private readonly IProvenanceLogger _logger;

        public Provenance(IProvenanceLogger logger)
        {
            _logger = logger;
        }

        public void Info(string action, object metadata = null, string correlationId = null) =>
            _logger.Log(new ProvenanceEvent
            {
                Action = action,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId ?? Guid.NewGuid().ToString("n"),
                Metadata = Enrich(metadata)
            });

        public void Error(string action, string error, object metadata = null, string correlationId = null) =>
            _logger.Log(new ProvenanceEvent
            {
                Action = action,
                Timestamp = DateTime.UtcNow,
                CorrelationId = correlationId ?? Guid.NewGuid().ToString("n"),
                Metadata = Enrich(new { Error = error, Extra = metadata })
            });

        public IDisposable Begin(string action, object metadata = null, string correlationId = null)
        {
            var cid = correlationId ?? Guid.NewGuid().ToString("n");
            Info(action + "Start", metadata, cid);
            return new Scope(this, action, cid);
        }

        private object Enrich(object metadata)
        {
            var asm = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            return new
            {
                metadata,
                Env = new
                {
                    Machine = Environment.MachineName,
                    User = Environment.UserName,
                    OS = Environment.OSVersion.ToString(),
                    ProcId = Process.GetCurrentProcess().Id,
                    App = asm.GetName().Name,
                    Version = asm.GetName().Version?.ToString()
                }
            };
        }

        private sealed class Scope : IDisposable
        {
            private readonly Provenance _p;
            private readonly string _action;
            private readonly string _cid;
            private bool _disposed;

            public Scope(Provenance p, string action, string cid)
            {
                _p = p; _action = action; _cid = cid;
            }

            public void Dispose()
            {
                if (_disposed) return;
                _p.Info(_action + "End", null, _cid);
                _disposed = true;
            }
        }
    }
}
