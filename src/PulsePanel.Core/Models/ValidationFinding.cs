using System.Text.Json.Serialization;

namespace PulsePanel.Core.Models;

public record ValidationFinding
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("severity")]
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Info;

    [JsonPropertyName("ruleId")]
    public string? RuleId { get; init; }
}
