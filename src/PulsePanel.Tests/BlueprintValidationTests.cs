using PulsePanel.Blueprints;
using Xunit;
using System.IO;
using PulsePanel.Core.Services;

namespace PulsePanel.Tests;

public class BlueprintValidationTests
{
    [Theory]
    [InlineData("blueprints/minecraft-java-paper")]
    public void ValidBlueprint_Should_Pass_Validation(string path)
    {
        // Arrange
        var validator = new BlueprintValidator();
        var blueprintPath = Path.GetFullPath(path);

        // Act
        var result = validator.Validate(blueprintPath);

        // Assert
        Assert.True(result.IsValid, $"Validation failed with errors: {string.Join(", ", result.Errors)}");
        Assert.Empty(result.Errors);
    }

    [Theory]
    [InlineData("blueprints/test-fail-bad-hash", "Hash mismatch for file")]
    [InlineData("blueprints/test-fail-missing-file", "listed in meta.yaml does not exist")]
    [InlineData("blueprints/test-fail-unknown-token", "used in templates but not defined")]
    public void InvalidBlueprint_Should_Fail_Validation(string path, string expectedError)
    {
        // Arrange
        var validator = new BlueprintValidator();
        var blueprintPath = Path.GetFullPath(path);

        // Act
        var result = validator.Validate(blueprintPath);

        // Assert
        Assert.False(result.IsValid);
        Assert.NotEmpty(result.Errors);
        Assert.Contains(result.Errors, e => e.Contains(expectedError));
    }
}