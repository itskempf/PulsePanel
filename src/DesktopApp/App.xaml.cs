using Microsoft.UI.Xaml;
using Microsoft.Extensions.DependencyInjection;
using PulsePanel.Core.Services;

namespace PulsePanel.DesktopApp
{
    public partial class App : Application
    {
        private Window m_window;

        public static IServiceProvider Services { get; private set; }

        public App()
        {
            this.InitializeComponent();
            ConfigureServices();
        }

        private void ConfigureServices()
        {
            var services = new ServiceCollection();

            // Register services
            services.AddSingleton<IStoragePathResolver, StoragePathResolver>();
            services.AddSingleton<IConfigEditor, ConfigEditor>();

            Services = services.BuildServiceProvider();
        }

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }
    }
}
