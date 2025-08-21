using PulsePanel.Blueprints.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
using System.Collections.Generic;
using PulsePanel.Core.Services;
using System.Threading.Tasks;
using PulsePanel.Blueprints;
using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Core.Services;

public class BlueprintValidator : IBlueprintValidator
{
    private readonly IProvenanceLogger? _logger;

    // A small list of common SPDX licenses for a basic check.
    private static readonly HashSet<string> CommonLicenses = new()
    {
        "MIT", "Apache-2.0", "GPL-3.0-only", "GPL-3.0-or-later", "BSD-3-Clause", "ISC"
    };

    public BlueprintValidator(IProvenanceLogger? logger = null)
    {
        _logger = logger;
    }

    public Task<ValidationResult> ValidateBlueprintAsync(string blueprintPath)
    {
        return Task.FromResult(Validate(blueprintPath));
    }

    public ValidationResult Validate(string blueprintPath)
    {
        var result = new ValidationResult();

        // 1. Check for meta.yaml and deserialize it
        var metaYamlPath = Path.Combine(blueprintPath, "meta.yaml");
        if (!File.Exists(metaYamlPath))
        {
            result.Errors.Add("Blueprint validation failed: 'meta.yaml' not found in the blueprint root.");
            return result;
        }

        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(CamelCaseNamingConvention.Instance)
            .Build();

        Blueprint blueprint;
        try
        {
            var yamlContent = File.ReadAllText(metaYamlPath);
            blueprint = deserializer.Deserialize<Blueprint>(yamlContent);
            if (blueprint == null)
            {
                result.Errors.Add("Blueprint validation failed: 'meta.yaml' is empty or invalid.");
                return result;
            }
            result.Blueprint = blueprint;
        }
        catch (System.Exception ex)
        {
            result.Errors.Add($"Blueprint validation failed: Failed to parse 'meta.yaml'. Error: {ex.Message}");
            return result;
        }

        // 2. Validate required fields
        if (string.IsNullOrWhiteSpace(blueprint.Name)) result.Errors.Add("Required field 'name' is missing or empty.");
        if (string.IsNullOrWhiteSpace(blueprint.Version)) result.Errors.Add("Required field 'version' is missing or empty.");
        if (string.IsNullOrWhiteSpace(blueprint.Author)) result.Errors.Add("Required field 'author' is missing or empty.");
        if (string.IsNullOrWhiteSpace(blueprint.License))
        {
            result.Errors.Add("Required field 'license' is missing or empty.");
            result.LicenseCheckResult = "Missing";
        }
        else
        {
            if (blueprint.License.Contains(" "))
            {
                result.Errors.Add("'license' field should be a valid SPDX identifier and not contain spaces.");
                result.LicenseCheckResult = "Invalid";
            }
            else if (CommonLicenses.Contains(blueprint.License))
            {
                result.LicenseCheckResult = "OK";
            }
            else
            {
                result.Warnings.Add($"License '{blueprint.License}' is not a common SPDX identifier. Please verify it is correct.");
                result.LicenseCheckResult = "Warning";
            }
        }

        if (blueprint.Provenance == null)
        {
            result.Errors.Add("Required section 'provenance' is missing.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(blueprint.Provenance.SourceUrl)) result.Errors.Add("Provenance field 'source_url' is missing or empty.");
            if (string.IsNullOrWhiteSpace(blueprint.Provenance.SourceHash)) result.Errors.Add("Provenance field 'source_hash' is missing or empty.");
        }

        // More validation logic will be added here...

        if (result.Errors.Count > 0) return result;

        // 3. Validate file hashes
        if (blueprint.Files != null)
        {
            foreach (var fileEntry in blueprint.Files)
            {
                if (string.IsNullOrWhiteSpace(fileEntry.Path))
                {
                    result.Errors.Add("A file entry in 'files' is missing a 'path'.");
                    continue;
                }

                var filePath = Path.Combine(blueprintPath, fileEntry.Path);
                if (!File.Exists(filePath))
                {
                    result.Errors.Add($"File '{fileEntry.Path}' listed in meta.yaml does not exist.");
                    continue;
                }

                var hashParts = fileEntry.Hash?.Split(':');
                if (hashParts?.Length != 2 || hashParts[0] != "sha256" || string.IsNullOrWhiteSpace(hashParts[1]))
                {
                    result.Errors.Add($"Invalid hash format for file '{fileEntry.Path}'. Expected 'sha256:<hash>'.");
                    continue;
                }
                var expectedHash = hashParts[1];

                var actualHash = ComputeSha256(filePath);
                if (!actualHash.Equals(expectedHash, System.StringComparison.OrdinalIgnoreCase))
                {
                    result.Errors.Add($"Hash mismatch for file '{fileEntry.Path}'. Expected '{expectedHash}', but got '{actualHash}'.");
                }
            }
        }

        if (result.Errors.Count > 0) return result;

        // 4. Validate tokens
        var definedTokens = blueprint.Tokens?.Select(t => t.Key).ToHashSet() ?? new HashSet<string>();
        var usedTokens = new HashSet<string>();
        var templatesPath = Path.Combine(blueprintPath, "templates");

        if (Directory.Exists(templatesPath))
        {
            var templateFiles = Directory.GetFiles(templatesPath, "*", SearchOption.AllDirectories);
            var tokenRegex = new Regex(@"\{\{([a-zA-Z0-9_.-]+)\}\}", RegexOptions.Compiled);

            foreach (var file in templateFiles)
            {
                var content = File.ReadAllText(file);
                var matches = tokenRegex.Matches(content);
                foreach (Match match in matches)
                {
                    usedTokens.Add(match.Groups[1].Value);
                }
            }
        }

        var undefinedTokens = usedTokens.Except(definedTokens);
        foreach (var token in undefinedTokens)
        {
            result.Errors.Add($"Token '{{{{{token}}}}}' is used in templates but not defined in meta.yaml.");
        }

        var unusedTokens = definedTokens.Except(usedTokens);
        foreach (var token in unusedTokens)
        {
            result.Warnings.Add($"Token '{{{{{token}}}}}' is defined in meta.yaml but not used in any template.");
        }

        if (result.Errors.Count == 0)
        {
            result.Status = "pass";
        }

        if (_logger != null && result.Blueprint != null)
        {
            var logEntry = new LogEntry
            {
                Action = "validate",
                Blueprint = new BlueprintInfo { Name = result.Blueprint.Name, Version = result.Blueprint.Version },
                Inputs = new InputsInfo { MetaPath = metaYamlPath },
                Results = new ResultsInfo
                {
                    Status = result.Status,
                    Errors = result.Errors.Select(e => new ResultDetail { Code = "VALIDATION_ERROR", Message = e }).ToList(),
                    Warnings = result.Warnings.Select(w => new ResultDetail { Code = "VALIDATION_WARNING", Message = w }).ToList(),
                },
                License = new LicenseInfo { Id = result.Blueprint.License, Compatible = result.LicenseCheckResult == "OK" }
            };
            _logger.Log(logEntry);
        }

        return result;
    }

    private string ComputeSha256(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = sha256.ComputeHash(stream);
        return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
