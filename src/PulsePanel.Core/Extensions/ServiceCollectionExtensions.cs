using Microsoft.Extensions.DependencyInjection;
using PulsePanel.Core.Events;
using PulsePanel.Core.Services;
using PulsePanel.Windows.Services;
using PulsePanel.App.Services;
using PulsePanel.App.ViewModels;
using PulsePanel.Core.Impl;

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
            services.AddSingleton<IWindowsServiceManager, WindowsServiceManager>();
            services.AddSingleton<ISteamCmdManager, SteamCmdManager>();
            services.AddSingleton<IFirewallManager, FirewallManager>();
            services.AddSingleton<IProvenanceLogger, ProvenanceLogger>();
            services.AddSingleton<IProvenance, Provenance>();
            services.AddSingleton<IEventBus, EventBus>();
            services.AddSingleton<IServerStore, ServerStore>();

            // Health monitoring
            services.AddSingleton<IServerProcessInspector, DefaultServerProcessInspector>();
            services.AddSingleton<IHealthMonitorService, HealthMonitorService>();

            // App-level services
            services.AddSingleton<ProvenanceReader>();

            // ViewModels
            services.AddTransient<ServersViewModel>();
            services.AddTransient<SettingsViewModel>();
            services.AddTransient<AuditLogViewModel>();

            return services;
        }
    }
}
