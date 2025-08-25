
using System;

namespace PulsePanel.Core.Models
{
    public class BackupRecord
    {
        public Guid Id { get; set; }
        public Guid TargetServerId { get; set; }
        public DateTime Timestamp { get; set; }
        public string FilePath { get; set; } = string.Empty;
        public long SizeInBytes { get; set; }
    }
}
