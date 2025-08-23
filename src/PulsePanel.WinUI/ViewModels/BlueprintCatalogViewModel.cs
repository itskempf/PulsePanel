using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PulsePanel.Core.Blueprints;
using PulsePanel.Core.Events;
using PulsePanel.Core.Provenance;
using System.Collections.ObjectModel;

public partial class BlueprintCatalogViewModel : ObservableObject
{
    private readonly IBlueprintCatalog _catalog;
    private readonly IBlueprintInstaller _installer;
    private readonly IEventPublisher _events;
    private readonly IProvenanceLogger _prov;

    [ObservableProperty] private ObservableCollection<Blueprint> items = new();
    [ObservableProperty] private string indexUrl = "https://example.com/blueprints/index.json";
    [ObservableProperty] private string statusMessage = "";

    public BlueprintCatalogViewModel(IBlueprintCatalog catalog, IBlueprintInstaller installer, IEventPublisher events, IProvenanceLogger prov)
    {
        _catalog = catalog;
        _installer = installer;
        _events = events;
        _prov = prov;
    }

    [RelayCommand]
    public async Task SyncAsync()
    {
        try
        {
            StatusMessage = "Syncing catalog...";
            var list = await _catalog.SyncAsync(new Uri(IndexUrl));
            Items = new ObservableCollection<Blueprint>(list);
            StatusMessage = $"Loaded {Items.Count} blueprints.";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    public async Task InstallAsync(Blueprint? bp)
    {
        if (bp == null) return;
        var startEvt = _events.Publish(BlueprintEvents.InstallStarted, new { bp.Id, bp.Name });
        await _prov.LogAsync("user", "Blueprint.Install.Start", bp.Id, new { bp.Name });

        try
        {
            StatusMessage = $"Installing {bp.Name}...";
            await _installer.InstallAsync(bp);
            _events.PublishFrom(BlueprintEvents.InstallSucceeded, startEvt, new { bp.Id });
            await _prov.LogAsync("user", "Blueprint.Install.Success", bp.Id, new { bp.Name });
            StatusMessage = $"Installed {bp.Name}.";
        }
        catch (Exception ex)
        {
            _events.PublishFrom(BlueprintEvents.InstallFailed, startEvt, new { bp.Id, ex.Message });
            await _prov.LogAsync("user", "Blueprint.Install.Fail", bp.Id, new { bp.Name, ex.Message });
            StatusMessage = $"Failed: {ex.Message}";
        }
    }
}
