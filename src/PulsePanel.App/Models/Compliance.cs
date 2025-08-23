using System.Collections.Generic;

namespace PulsePanel.App.Models
{
    public enum ComplianceStatus { Pass, Warn, Fail }

    public sealed class ComplianceRule
    {
        public string Id { get; init; } = "";
        public string Description { get; init; } = "";
        public string CheckType { get; init; } = ""; // e.g., "FileExists", "ServiceRunning", "VersionMatch"
        public string Target { get; init; } = "";    // path/service/package
        public string? Expected { get; init; }
    }

    public sealed class ComplianceResult
    {
        public ComplianceRule Rule { get; init; } = new();
        public ComplianceStatus Status { get; init; }
        public string? Message { get; init; }
    }

    public sealed class ComplianceReport
    {
        public string BlueprintName { get; init; } = "";
        public string? NodeId { get; init; }
        public List<ComplianceResult> Results { get; init; } = new();
        public ComplianceStatus Overall { get; init; }
    }
}