namespace PulsePanel.Core.Services
{
    public sealed class ServerProcessInfo
    {
        public string ServerId { get; set; } = string.Empty;
        public int ProcessId { get; set; }
        public string? ExecutablePath { get; set; }
    }
}
