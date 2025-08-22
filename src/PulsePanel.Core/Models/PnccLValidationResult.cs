using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace PulsePanel.Core.Models;

public record PnccLValidationResult
{
    [JsonPropertyName("isValid")]
    public bool IsValid => !Findings.Any(f => f.Severity == ValidationSeverity.Error);

    [JsonPropertyName("findings")]
    public List<ValidationFinding> Findings { get; init; } = new();

    [JsonPropertyName("rulesetVersionHash")]
    public string RulesetVersionHash { get; init; } = string.Empty;

    public PnccLValidationResult AddFinding(ValidationFinding finding)
    {
        Findings.Add(finding);
        return this;
    }

    public PnccLValidationResult AddFindings(IEnumerable<ValidationFinding> findings)
    {
        Findings.AddRange(findings);
        return this;
    }
}
