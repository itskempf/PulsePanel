using Microsoft.Extensions.Configuration;
using PulsePanel.Api.Models;

namespace PulsePanel.Api.Services;

public class SettingsService
{
    private readonly IConfiguration _config;

    public SettingsService(IConfiguration config)
    {
        _config = config;
    }

    public PulsePanelSettings GetSettings()
    {
        // The IConfiguration object automatically handles layering of appsettings.json, environment variables, etc.
        // We just need to bind the section to our model.
        var settings = new PulsePanelSettings();
        _config.GetSection("PulsePanel").Bind(settings);
        return settings;
    }

    public void SaveSettings(PulsePanelSettings settings)
    {
        // Note: Persisting settings back to a file is complex and has security implications.
        // For this sprint, we will not implement the save functionality.
        // A full implementation would likely write to a user-specific override file.
        throw new NotImplementedException("Saving settings is not supported in this version.");
    }
}
