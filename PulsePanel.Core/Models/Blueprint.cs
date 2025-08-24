namespace PulsePanel.Core.Models;

public class Blueprint
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public List<BlueprintVariable> Variables { get; set; } = new();
}