using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PulsePanel.Blueprints.Provenance;

public record LogEntry
{
    [JsonPropertyName("ts")]
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    [JsonPropertyName("actor")]
    public string Actor { get; init; } = "system";

    [JsonPropertyName("action")]
    public string Action { get; init; } = string.Empty;

    [JsonPropertyName("blueprint")]
    public BlueprintInfo Blueprint { get; init; } = new();

    [JsonPropertyName("inputs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public InputsInfo? Inputs { get; init; }

    [JsonPropertyName("results")]
    public ResultsInfo Results { get; init; } = new();

    [JsonPropertyName("artifacts")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public ArtifactsInfo? Artifacts { get; init; }

    [JsonPropertyName("license")]
    public LicenseInfo License { get; init; } = new();

    [JsonPropertyName("env")]
    public EnvironmentInfo Env { get; init; } = new();
}

public record BlueprintInfo
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("version")]
    public string Version { get; init; } = string.Empty;
}

public record InputsInfo
{
    [JsonPropertyName("metaPath")]
    public string MetaPath { get; init; } = string.Empty;

    [JsonPropertyName("valuesHash")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? ValuesHash { get; init; }
}

public record ResultsInfo
{
    [JsonPropertyName("status")]
    public string Status { get; init; } = "fail";

    [JsonPropertyName("errors")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ResultDetail> Errors { get; init; } = new();

    [JsonPropertyName("warnings")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ResultDetail> Warnings { get; init; } = new();
}

public record ResultDetail
{
    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}

public record ArtifactsInfo
{
    [JsonPropertyName("sources")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ArtifactDetail> Sources { get; init; } = new();

    [JsonPropertyName("outputs")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public List<ArtifactDetail> Outputs { get; init; } = new();
}

public record ArtifactDetail
{
    [JsonPropertyName("path")]
    public string Path { get; init; } = string.Empty;

    [JsonPropertyName("hash")]
    public string Hash { get; init; } = string.Empty;
}

public record LicenseInfo
{
    [JsonPropertyName("id")]
    public string Id { get; init; } = string.Empty;

    [JsonPropertyName("compatible")]
    public bool Compatible { get; init; }
}

public record EnvironmentInfo
{
    [JsonPropertyName("platform")]
    public string Platform { get; init; } = Environment.OSVersion.Platform.ToString();

    [JsonPropertyName("appVersion")]
    public string AppVersion { get; init; } = "0.1.0"; // Placeholder

    [JsonPropertyName("gitCommit")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? GitCommit { get; init; } // Placeholder
}
