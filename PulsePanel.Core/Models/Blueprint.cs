
namespace PulsePanel.Core.Models
{
    public class Blueprint
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ProvisionerType { get; set; } = string.Empty;
        public string DownloadUrl { get; set; } = string.Empty;
        public int SteamAppId { get; set; }
        public string ExecutablePath { get; set; } = string.Empty;
        public string Arguments { get; set; } = string.Empty;
    }
}
