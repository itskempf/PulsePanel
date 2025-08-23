using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using PulsePanel.Core.Extensions;
using PulsePanel.Core.Services;
using System;
using System.IO;
using PulsePanel.App.State;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; } = default!;
        private CancellationTokenSource? _healthCts;

        public App()
        {
            InitializeComponent();
            var sc = new ServiceCollection();
            sc.AddPulsePanelServices();
            Services = sc.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            // Start health monitor
            var health = Services.GetRequiredService<IHealthMonitorService>();
            _healthCts = new CancellationTokenSource();
            _ = health.StartAsync(_healthCts.Token);

            base.OnLaunched(args);
            // Initialize shared state
            var appState = Services.GetRequiredService<AppState>();
            var serverService = Services.GetRequiredService<IServerService>();
            _ = appState.InitializeAsync(serverService);

            var window = new MainWindow();
            window.Activate();
        }

        protected override void OnSuspending(object sender, SuspendingEventArgs e)
        {
            _healthCts?.Cancel();
            base.OnSuspending(sender, e);
        }
    }
}
