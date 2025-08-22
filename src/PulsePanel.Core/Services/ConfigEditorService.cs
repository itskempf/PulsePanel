using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using PulsePanel.Blueprints.Provenance;
using PulsePanel.Core.Models;

namespace PulsePanel.Core.Services;

/// <summary>
/// Config Editor v1.0 service: staged apply -> PNCCL preflight -> commit/rollback.
/// Guardrails: no live apply on PNCCL errors; provenance records ruleset version/hash per commit.
/// </summary>
public class ConfigEditorService : IConfigEditorService
{
    private readonly IConfigValidationProvider _validationProvider;
    private readonly IConfigDiffService _diffService;
    private readonly IConfigProfileStore _profileStore;
    private readonly IComplianceOverlayMapper _overlayMapper;
    private readonly ProvenanceLogger _provenanceLogger;

    public ConfigEditorService(
        IConfigValidationProvider validationProvider,
        IConfigDiffService diffService,
        IConfigProfileStore profileStore,
        IComplianceOverlayMapper overlayMapper,
        ProvenanceLogger provenanceLogger)
    {
        _validationProvider = validationProvider;
        _diffService = diffService;
        _profileStore = profileStore;
        _overlayMapper = overlayMapper;
        _provenanceLogger = provenanceLogger;
    }

    public async Task<string> LoadConfigAsync(string filePath)
    {
        return await File.ReadAllTextAsync(filePath);
    }

    public async Task<PnccLValidationResult> ValidateConfigAsync(string filePath, string content)
    {
        var fileType = InferTypeFromExtension(filePath);
        return await _validationProvider.ValidateAsync(content, fileType);
    }

    public async Task StageChangesAsync(string filePath, string content)
    {
        var stagedPath = GetStagedPath(filePath);
        Directory.CreateDirectory(Path.GetDirectoryName(stagedPath) ?? ".");
        await File.WriteAllTextAsync(stagedPath, content);
    }

    public async Task<CommitResult> CommitChangesAsync(string filePath, string commitMessage)
    {
        var stagedPath = GetStagedPath(filePath);
        if (!File.Exists(stagedPath))
        {
            return new CommitResult { Success = false, CommitId = string.Empty, ProvenanceId = string.Empty };
        }

        string content = await File.ReadAllTextAsync(stagedPath);
        PnccLValidationResult validation;
        bool validatorOffline = false;
        try
        {
            validation = await ValidateConfigAsync(filePath, content);
        }
        catch (Exception)
        {
            // Offline validator mode
            validatorOffline = true;
            validation = new PnccLValidationResult { RulesetVersionHash = "unknown/offline" };
        }

        // Guardrail: no live apply on errors or when validator is offline
        var hasErrors = !validation.IsValid;
        if (validatorOffline || hasErrors)
        {
            await _provenanceLogger.LogAsync(new LogEntry
            {
                Action = "ConfigCommitBlocked",
                EntityType = "ConfigFile",
                EntityIdentifier = filePath,
                EntityHash = ComputeSha256Hex(content),
                Results = new ResultsInfo
                {
                    Status = validatorOffline ? "validator_offline" : "blocked",
                    Errors = new List<ResultDetail>
                    {
                        new ResultDetail
                        {
                            Code = validatorOffline ? "PNCCL-OFFLINE" : "PNCCL-ERRORS",
                            Message = validatorOffline ? "Validator not available" : "PNCCL errors present"
                        }
                    }
                },
                Metadata = new Dictionary<string, object>
                {
                    { "rulesetVersionHash", validation.RulesetVersionHash }
                }
            });

            return new CommitResult { Success = false, CommitId = string.Empty, ProvenanceId = string.Empty };
        }

        // Apply commit
        var commitId = ComputeSha256Hex(content);
        var provenanceId = Guid.NewGuid().ToString("N");

        // Atomic-ish replace: delete target then move staged
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        File.Move(stagedPath, filePath);

        await _provenanceLogger.LogAsync(new LogEntry
        {
            Action = "ConfigCommitted",
            EntityType = "ConfigFile",
            EntityIdentifier = filePath,
            EntityHash = commitId,
            Results = new ResultsInfo
            {
                Status = "pass"
            },
            Metadata = new Dictionary<string, object>
            {
                { "commitMessage", commitMessage },
                { "rulesetVersionHash", validation.RulesetVersionHash },
                { "findings", validation.Findings }
            }
        });

        return new CommitResult { Success = true, CommitId = commitId, ProvenanceId = provenanceId };
    }

    public Task RollbackChangesAsync(string filePath)
    {
        var stagedPath = GetStagedPath(filePath);
        if (File.Exists(stagedPath))
        {
            File.Delete(stagedPath);
        }
        return Task.CompletedTask;
    }

    public async Task<ConfigDiff> DiffWithBlueprintAsync(string filePath)
    {
        // Placeholder: compare file to itself until default blueprint content is wired
        var current = await LoadConfigAsync(filePath);
        return await _diffService.GenerateDiffAsync(current, current);
    }

    public async Task<ConfigDiff> DiffWithProfileAsync(string filePath, string profileName)
    {
        var current = await LoadConfigAsync(filePath);
        var profile = await _profileStore.LoadProfileAsync(filePath, profileName);
        var newContent = SerializeProfile(profile);
        return await _diffService.GenerateDiffAsync(current, newContent);
    }

    private static string GetStagedPath(string filePath) => filePath + ".staged";

    private static string InferTypeFromExtension(string filePath)
    {
        var ext = Path.GetExtension(filePath).ToLowerInvariant();
        return ext switch
        {
            ".json" => "json",
            ".yaml" or ".yml" => "yaml",
            ".ini" => "ini",
            ".xml" => "xml",
            ".cfg" => "cfg",
            ".properties" => "properties",
            _ => "text"
        };
    }

    private static string SerializeProfile(ConfigProfile profile)
    {
        // Minimal, stable string for diffing; can be replaced by per-format generators later
        var sb = new StringBuilder();
        sb.AppendLine($"# Profile: {profile.Name}");
        if (profile.Settings != null)
        {
            foreach (var kvp in profile.Settings)
            {
                sb.AppendLine($"{kvp.Key}={kvp.Value}");
            }
        }
        return sb.ToString();
    }

    private static string ComputeSha256Hex(string content)
    {
        using var sha = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(content);
        var hash = sha.ComputeHash(bytes);
        var sb = new StringBuilder(hash.Length * 2);
        foreach (var b in hash)
            sb.Append(b.ToString("x2"));
        return sb.ToString();
    }
}
