
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PulsePanel.Core.Services
{
    public sealed class ProvenanceLogger : IProvenanceLogger
    {
        private readonly string _file;
        private readonly SemaphoreSlim _gate = new(1,1);
        public ProvenanceLogger()
        {
            var baseDir = AppContext.BaseDirectory;
            var dataDir = Path.Combine(baseDir, "Data");
            Directory.CreateDirectory(dataDir);
            _file = Path.Combine(dataDir, "provenance.log");
        }
        public async Task LogAsync(string action, string detail)
        {
            var line = $"{DateTime.UtcNow:O}	{action}	{detail}{Environment.NewLine}";
            await _gate.WaitAsync();
            try { await File.AppendAllTextAsync(_file, line, Encoding.UTF8); }
            finally { _gate.Release(); }
        }
    }
}
