using System.Text.Json.Serialization;

namespace PulsePanel.Core.Models;

public class PulsePanelSettings
{
    [JsonPropertyName("provenanceLogPath")]
    public string ProvenanceLogPath { get; set; } = string.Empty;

    [JsonPropertyName("defaultInstallRoot")]
    public string DefaultInstallRoot { get; set; } = string.Empty;

    [JsonPropertyName("steamCmdPath")]
    public string SteamCmdPath { get; set; } = string.Empty;
}
