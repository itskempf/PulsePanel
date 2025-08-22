using Microsoft.UI.Xaml;
using PulsePanel.App.Services;
using PulsePanel.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using PulsePanel.App.State;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App
{
    public partial class App : Application
    {
    public static Window? MainAppWindow { get; private set; }
    public static INavigationService NavigationService { get; private set; } = new NavigationService();
    public static IServiceProvider Services { get; private set; } = default!;
        
        public App()
        {
            InitializeComponent();
            ConfigureServices();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            // Initialize shared state
            var appState = Services.GetRequiredService<AppState>();
            var serverService = Services.GetRequiredService<IServerService>();
            _ = appState.InitializeAsync(serverService);

            var window = new MainWindow();
            MainAppWindow = window;
            window.Activate();
        }

        private static void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Core singletons
            services.AddSingleton<ProvenanceLogger>(_ => new ProvenanceLogger(Path.Combine(AppContext.BaseDirectory, "logs", "provenance.jsonl")));
            services.AddSingleton<ServerStore>();
            services.AddSingleton<ServerProcessService>();
            services.AddSingleton<IServerService, ServerService>();
            services.AddSingleton<AppState>();
            services.AddTransient<ServersViewModel>();

            // App services
            services.AddSingleton<INavigationService>(NavigationService);

            Services = services.BuildServiceProvider();
        }
    }
}
