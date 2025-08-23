namespace PulsePanel.App.Models
{
    public record HealthSnapshot(System.DateTime Timestamp, double CpuPercent, double MemoryMb, System.TimeSpan Uptime);
}
