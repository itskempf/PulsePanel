using Microsoft.UI.Xaml;
using PulsePanel.App.Services;

namespace PulsePanel.App
{
    public partial class App : Application
    {
        public static Window MainAppWindow { get; private set; }
        public static INavigationService NavigationService { get; private set; }
        
        public App()
        {
            this.InitializeComponent();
            NavigationService = new NavigationService();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);
            var window = new MainWindow();
            MainAppWindow = window;
            window.Activate();
        }
    }
}
