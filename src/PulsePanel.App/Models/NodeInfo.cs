using System;

namespace PulsePanel.App.Models
{
    public enum NodeStatus { Unknown, Online, Offline }

    public sealed class NodeInfo
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string Name { get; init; } = "Unnamed";
        public string Url { get; init; } = "http://localhost:5070";
        public string OS { get; init; } = Environment.OSVersion.ToString();
        public DateTime LastCheckIn { get; set; } = DateTime.UtcNow;
        public NodeStatus Status { get; set; } = NodeStatus.Unknown;
        public string? ApiKey { get; init; }
    }
}