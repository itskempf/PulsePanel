using System.Text.Json.Serialization;

namespace PulsePanel.Core.Models;

public record GameInstall(int AppId);

public record GameStart(
    string Exec,
    string Args
);

public record GameDefinition(
    string Id,
    string Label,
    GameInstall Install,
    GameStart Start,
    Dictionary<string,int> DefaultPorts,
    Dictionary<string,object> Tokens,
    List<TemplateSpec> Templates
);

public record TemplateSpec(
    string Target,
    string Content
);

public class ServerEntry {
    public string Id { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string Game { get; set; } = default!;
    public string Status { get; set; } = "created";
    public string CreatedAt { get; set; } = DateTimeOffset.UtcNow.ToString("o");
    public int? Pid { get; set; }
    public Dictionary<string, int> Ports { get; set; } = new();
    public Dictionary<string, object> Tokens { get; set; } = new();
    public string InstallDir { get; set; } = default!;
}
