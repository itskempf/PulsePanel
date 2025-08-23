using System;
using System.IO;

namespace PulsePanel.Core.Services
{
    public class StorageManager : IStorageManager
    {
        private readonly IProvenanceLogger _logger;
        private string _path;

        public StorageManager(IProvenanceLogger logger)
        {
            _logger = logger;
            _path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PulsePanel");
            Directory.CreateDirectory(_path);
        }

        public string GetStoragePath() => _path;

        public void SetStoragePath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("Storage path cannot be empty");

            _path = path;
            Directory.CreateDirectory(_path);

            _logger.Log(new LogEntry
            {
                Action = "StoragePathChanged",
                Timestamp = DateTime.UtcNow,
                Metadata = new System.Collections.Generic.Dictionary<string, object>
                {
                    { "NewPath", _path }
                }
            });
        }
    }
}
