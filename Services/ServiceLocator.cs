using Microsoft.Extensions.DependencyInjection;
using PulsePanel.Core.Services;
using PulsePanel.ViewModels; // CreateServerViewModel
using Pulse_Panel.ViewModels; // ServersPageViewModel

namespace PulsePanel.Services;

public class ServiceLocator
{
    private static IServiceProvider? _provider;

    public static void ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register Core Services
        services.AddSingleton<IServerInstanceRepository, ServerInstanceRepository>();
        services.AddSingleton<IServerService, ServerService>();
        services.AddSingleton<IProvisioningService, ProvisioningService>();
        services.AddSingleton<NavigationService>();

        // Register ViewModels
        services.AddTransient<ServersPageViewModel>();
        services.AddTransient<CreateServerViewModel>();

        _provider = services.BuildServiceProvider();
    }

    public static T Get<T>() where T : notnull
    {
        if (_provider == null)
        {
            throw new InvalidOperationException("Services have not been configured.");
        }
        return _provider.GetRequiredService<T>();
    }
}