using System;
using System.Collections.Generic;

namespace PulsePanel.App.Models
{
    public class Blueprint
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public string Version { get; set; } = "1.0.0";
        public string Description { get; set; } = string.Empty;

        // Metadata for provenance
        public string Author { get; set; } = string.Empty;
        public string License { get; set; } = string.Empty;
        public string Checksum { get; set; } = string.Empty;

        // Execution steps
        public List<BlueprintStep> InstallSteps { get; set; } = new();
        public List<BlueprintStep> UpdateSteps { get; set; } = new();
        public List<BlueprintStep> ValidateSteps { get; set; } = new();
    }

    public class BlueprintStep
    {
        public string Action { get; set; } = string.Empty; // e.g. "CopyFile", "RunCommand"
        public Dictionary<string, string> Parameters { get; set; } = new();
    }
}