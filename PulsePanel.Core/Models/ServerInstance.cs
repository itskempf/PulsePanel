namespace PulsePanel.Core.Models;

public class ServerInstance
{
    public Guid Id { get; set; }
    public string InstanceName { get; set; } = string.Empty;
    public string GameName { get; set; } = string.Empty;
    public ServerStatus Status { get; set; }
    public string InstallPath { get; set; } = string.Empty;
    public int HealthScore { get; set; }
}
