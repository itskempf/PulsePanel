using PulsePanel.Blueprints.Models;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using PulsePanel.Core.Services;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Blueprints;
using PulsePanel.Blueprints.Provenance;

namespace PulsePanel.Core.Services;

public class ConfigGenerator : IConfigGenerator
{
    private readonly IProvenanceLogger? _logger;
    private readonly IBlueprintValidator _validator;

    public ConfigGenerator(IProvenanceLogger? logger = null, IBlueprintValidator? validator = null)
    {
        _logger = logger;
        _validator = validator ?? new BlueprintValidator(logger);
    }

    public Task<GenerationResult> GenerateAsync(string blueprintPath, string valuesPath, string outputRoot)
    {
        return Task.FromResult(Generate(blueprintPath, valuesPath, outputRoot));
    }

    public GenerationResult Generate(string blueprintPath, string valuesPath, string outputRoot)
    {
        var result = new GenerationResult();

        // 1. Validate the blueprint first
        var validationResult = ((BlueprintValidator)_validator).Validate(blueprintPath);
        if (!validationResult.IsValid)
        {
            result.Errors.Add("Blueprint is not valid. Please run 'validate-blueprint' for details.");
            result.Errors.AddRange(validationResult.Errors);
            return result;
        }
        var blueprint = validationResult.Blueprint!;

        // 2. Load values file
        if (!File.Exists(valuesPath))
        {
            result.Errors.Add($"Values file not found at '{valuesPath}'.");
            return result;
        }

        Dictionary<string, object> values;
        var valuesContent = File.ReadAllText(valuesPath);
        try
        {
            values = LoadValues(valuesPath, valuesContent);
        }
        catch (System.Exception ex)
        {
            result.Errors.Add($"Failed to parse values file '{valuesPath}'. Error: {ex.Message}");
            return result;
        }

        // 3. Determine deterministic output path
        var valuesHash = ComputeSha256(valuesContent);
        var outputPath = Path.Combine(outputRoot, blueprint.Name, blueprint.Version, valuesHash);
        result.OutputPath = outputPath;
        Directory.CreateDirectory(outputPath);

        // 4. Process templates
        var replacer = new TokenReplacer(values);
        var templatesPath = Path.Combine(blueprintPath, "templates");
        if (Directory.Exists(templatesPath))
        {
            var templateFiles = Directory.GetFiles(templatesPath, "*", SearchOption.AllDirectories);
            foreach (var templateFile in templateFiles)
            {
                var fileInfo = new FileInfo(templateFile);
                if (fileInfo.Length > 2 * 1024 * 1024) // 2MB limit
                {
                    result.Errors.Add($"Template file '{templateFile}' exceeds the 2MB size limit.");
                    continue;
                }

                var content = File.ReadAllText(templateFile);
                var processedContent = replacer.Replace(content);

                var relativePath = Path.GetRelativePath(templatesPath, templateFile);
                var destinationPath = Path.Combine(outputPath, relativePath);

                Directory.CreateDirectory(Path.GetDirectoryName(destinationPath)!);
                File.WriteAllText(destinationPath, processedContent);

                var outputHash = ComputeSha256(processedContent);
                result.GeneratedFileHashes[relativePath] = outputHash;
            }
        }

        result.Success = true;

        if (_logger != null)
        {
            var logEntry = new LogEntry
            {
                Action = "generate",
                Blueprint = new BlueprintInfo { Name = blueprint.Name, Version = blueprint.Version },
                Inputs = new InputsInfo { MetaPath = validationResult.Blueprint!.Provenance.SourceUrl, ValuesHash = $"sha256:{valuesHash}" },
                Results = new ResultsInfo { Status = "pass" },
                Artifacts = new ArtifactsInfo
                {
                    Outputs = result.GeneratedFileHashes.Select(kvp => new ArtifactDetail { Path = kvp.Key, Hash = $"sha256:{kvp.Value}" }).ToList()
                },
                License = new LicenseInfo { Id = blueprint.License, Compatible = validationResult.LicenseCheckResult == "OK" }
            };
            _logger.Log(logEntry);
        }

        return result;
    }

    private Dictionary<string, object> LoadValues(string filePath, string fileContent)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();
        if (extension == ".json")
        {
            return JsonSerializer.Deserialize<Dictionary<string, object>>(fileContent)!;
        }
        if (extension == ".yaml" || extension == ".yml")
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();
            // YamlDotNet deserializes to Dictionary<object, object>, need to convert
            var yamlObject = deserializer.Deserialize<object>(fileContent);
            var jsonOutput = JsonSerializer.Serialize(yamlObject);
            return JsonSerializer.Deserialize<Dictionary<string, object>>(jsonOutput)!;
        }

        throw new System.NotSupportedException($"Unsupported values file format: {extension}");
    }

    private string ComputeSha256(string input)
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hashBytes = sha256.ComputeHash(bytes);
        return System.BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }
}
