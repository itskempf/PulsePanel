using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using PulsePanel.Core.Models;
using PulsePanel.Core.Services;
using Xunit;

namespace PulsePanel.Tests;

public class ConfigEditorServiceTests
{
    private class FakeValidator : IConfigValidationProvider
    {
        private readonly Func<string, string, Task<PnccLValidationResult>> _fn;
        public FakeValidator(Func<string, string, Task<PnccLValidationResult>> fn) => _fn = fn;
        public Task<PnccLValidationResult> ValidateAsync(string content, string fileType) => _fn(content, fileType);
    }

    private class FakeDiff : IConfigDiffService
    {
        public Task<ConfigDiff> GenerateDiffAsync(string originalContent, string newContent)
            => Task.FromResult(new ConfigDiff { Lines = new List<DiffLine>() });
    }

    private class FakeProfiles : IConfigProfileStore
    {
        public Task DeleteProfileAsync(string configFilePath, string profileName) => Task.CompletedTask;
        public Task<IEnumerable<string>> GetProfileNamesAsync(string configFilePath) => Task.FromResult<IEnumerable<string>>(new List<string>());
        public Task<ConfigProfile> LoadProfileAsync(string configFilePath, string profileName) => Task.FromResult(new ConfigProfile { Name = profileName, Settings = new Dictionary<string, object>() });
        public Task SaveProfileAsync(string configFilePath, ConfigProfile profile) => Task.CompletedTask;
    }

    private class PassthroughOverlay : IComplianceOverlayMapper
    {
        public IEnumerable<ComplianceOverlayItem> Map(PnccLValidationResult validationResult) => new List<ComplianceOverlayItem>();
    }

    private static ProvenanceLogger CreateTempLogger(string testName)
    {
        var path = Path.Combine(Path.GetTempPath(), $"pprov_{testName}_{Guid.NewGuid():N}.log");
        return new ProvenanceLogger(path);
    }

    private static string CreateTempFileWithContent(string content)
    {
        var path = Path.Combine(Path.GetTempPath(), $"cfg_{Guid.NewGuid():N}.txt");
        File.WriteAllText(path, content);
        return path;
    }

    [Fact]
    public async Task Commit_Succeeds_When_NoErrors()
    {
        var validator = new FakeValidator((c, t) => Task.FromResult(new PnccLValidationResult { RulesetVersionHash = "hash" }));
        var svc = new ConfigEditorService(validator, new FakeDiff(), new FakeProfiles(), new PassthroughOverlay(), CreateTempLogger("ok"));
        var file = CreateTempFileWithContent("a=1");
        await svc.StageChangesAsync(file, "a=2");
        var result = await svc.CommitChangesAsync(file, "test");
        Assert.True(result.Success);
        Assert.True(File.Exists(file));
        Assert.Equal("a=2", await File.ReadAllTextAsync(file));
    }

    [Fact]
    public async Task Commit_Succeeds_With_Warnings()
    {
        var vr = new PnccLValidationResult { RulesetVersionHash = "hash" };
        vr.AddFinding(new ValidationFinding { Severity = ValidationSeverity.Warning, Code = "W", Message = "warn" });
        var validator = new FakeValidator((c, t) => Task.FromResult(vr));
        var svc = new ConfigEditorService(validator, new FakeDiff(), new FakeProfiles(), new PassthroughOverlay(), CreateTempLogger("warn"));
        var file = CreateTempFileWithContent("a=1");
        await svc.StageChangesAsync(file, "a=3");
        var result = await svc.CommitChangesAsync(file, "test");
        Assert.True(result.Success);
        Assert.Equal("a=3", await File.ReadAllTextAsync(file));
    }

    [Fact]
    public async Task Commit_Blocked_On_Errors()
    {
        var vr = new PnccLValidationResult { RulesetVersionHash = "hash" };
        vr.AddFinding(new ValidationFinding { Severity = ValidationSeverity.Error, Code = "E", Message = "err" });
        var validator = new FakeValidator((c, t) => Task.FromResult(vr));
        var svc = new ConfigEditorService(validator, new FakeDiff(), new FakeProfiles(), new PassthroughOverlay(), CreateTempLogger("err"));
        var file = CreateTempFileWithContent("a=1");
        await svc.StageChangesAsync(file, "a=4");
        var result = await svc.CommitChangesAsync(file, "test");
        Assert.False(result.Success);
        // original file content should remain
        Assert.Equal("a=1", await File.ReadAllTextAsync(file));
    }

    [Fact]
    public async Task Commit_Blocked_When_Validator_Offline()
    {
        var validator = new FakeValidator((c, t) => throw new Exception("offline"));
        var svc = new ConfigEditorService(validator, new FakeDiff(), new FakeProfiles(), new PassthroughOverlay(), CreateTempLogger("off"));
        var file = CreateTempFileWithContent("a=1");
        await svc.StageChangesAsync(file, "a=5");
        var result = await svc.CommitChangesAsync(file, "test");
        Assert.False(result.Success);
        Assert.Equal("a=1", await File.ReadAllTextAsync(file));
    }
}
