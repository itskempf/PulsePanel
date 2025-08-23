namespace PulsePanel.Core.Plugins;

public sealed record PluginMeta(
    string Name,
    string Version,
    string License,
    string Attribution,
    bool Enabled
);

public interface IPluginRegistry
{
    Task<IReadOnlyList<PluginMeta>> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(IEnumerable<PluginMeta> metas, CancellationToken ct = default);
    Task<bool> IsEnabledAsync(string name, CancellationToken ct = default);
}
