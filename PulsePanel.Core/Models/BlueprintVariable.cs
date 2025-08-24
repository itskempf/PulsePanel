namespace PulsePanel.Core.Models;

public class BlueprintVariable
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Type { get; set; } = "string";
    public string DefaultValue { get; set; } = string.Empty;
}