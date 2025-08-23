using System.Collections.Generic;

namespace PulsePanel.App.Models
{
    public sealed class Blueprint
    {
        public string Name { get; init; } = "";
        public string Version { get; init; } = "";
        public string Description { get; init; } = "";
        public IEnumerable<BlueprintAction> InstallActions { get; init; } = new List<BlueprintAction>();
        public IEnumerable<BlueprintAction> UpdateActions { get; init; } = new List<BlueprintAction>();
        public IEnumerable<BlueprintAction> ValidateActions { get; init; } = new List<BlueprintAction>();
        public List<ComplianceRule>? ComplianceRules { get; set; }
    }

    public sealed class BlueprintAction
    {
        public string Type { get; init; } = "";
        public string Description { get; init; } = "";
        public Dictionary<string, string>? Parameters { get; init; }
    }
}