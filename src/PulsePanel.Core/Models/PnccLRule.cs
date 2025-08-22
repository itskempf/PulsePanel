using System.Text.Json.Serialization;

namespace PulsePanel.Core.Models;

public record PnccLRule
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Warning;

    // Placeholder for rule logic. This would typically be a delegate or an interface
    // that the checker can invoke to evaluate the rule against an entity.
    // For now, we'll just define the metadata of the rule.
}
