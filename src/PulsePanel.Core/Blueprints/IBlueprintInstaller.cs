namespace PulsePanel.Core.Blueprints;

public interface IBlueprintInstaller
{
    Task InstallAsync(Blueprint bp, CancellationToken ct = default);
}
