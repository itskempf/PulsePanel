
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services
{
    public sealed class BackupService : IBackupService
    {
        private readonly IProvenanceLogger _log;
        private readonly IPnccLChecker _pncc;
        private readonly string _dir;
        private readonly string _file;
        public BackupService(IProvenanceLogger log, IPnccLChecker pncc)
        {
            _log = log; _pncc = pncc;
            var data = Path.Combine(AppContext.BaseDirectory, "Data");
            Directory.CreateDirectory(data);
            _dir = Path.Combine(data, "Backups");
            Directory.CreateDirectory(_dir);
            _file = Path.Combine(data, "backups.json");
        }
        public async Task<IReadOnlyList<BackupRecord>> GetBackupsAsync()
        {
            if (!File.Exists(_file)) return Array.Empty<BackupRecord>();
            var json = await File.ReadAllTextAsync(_file);
            return string.IsNullOrWhiteSpace(json) ? Array.Empty<BackupRecord>() : (JsonSerializer.Deserialize<List<BackupRecord>>(json) ?? new());
        }
        public async Task<BackupRecord> CreateBackupAsync(ServerInstance server)
        {
            if (!await _pncc.ValidateAsync("backup.create", server.Id.ToString()))
                throw new InvalidOperationException("PNCCL validation failed");

            var id = Guid.NewGuid();
            var zip = Path.Combine(_dir, $"{id}.zip");
            ZipFile.CreateFromDirectory(server.InstallPath, zip, CompressionLevel.Optimal, true);
            var size = new FileInfo(zip).Length;
            var rec = new BackupRecord { Id = id, TargetServerId = server.Id, Timestamp = DateTime.UtcNow, FilePath = zip, SizeInBytes = size };
            var list = (await GetBackupsAsync()).ToList();
            list.Add(rec);
            await File.WriteAllTextAsync(_file, JsonSerializer.Serialize(list, new JsonSerializerOptions{WriteIndented=true}));
            await _log.LogAsync("backup.created", zip);
            return rec;
        }
    }
}
