using Microsoft.Extensions.DependencyInjection;
using PulsePanel.Core.Services;
using PulsePanel.Windows.Services;

namespace PulsePanel.Core.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPulsePanelServices(this IServiceCollection services)
        {
            // Core services
            services.AddSingleton<IConfigValidationProvider, ConfigValidationProvider>();
            services.AddSingleton<IConfigDiffService, ConfigDiffService>();
            services.AddSingleton<IServerProcessService, ServerProcessService>();
            services.AddSingleton<IStorageManager, StorageManager>();

            // Windows integrations
            services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
            services.AddSingleton<ISteamCmdManager, SteamCmdManager>();
            services.AddSingleton<IFirewallManager, FirewallManager>();

            // Provenance (base + wrapper)
            services.AddSingleton<IProvenanceLogger, ProvenanceLogger>();
            services.AddSingleton<IProvenance, Provenance>();

            // ViewModels (UI may request)
            services.AddSingleton<MainViewModel>();
            services.AddSingleton<ServersViewModel>();

            return services;
        }
    }
}
