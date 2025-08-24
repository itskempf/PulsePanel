using Microsoft.UI.Xaml;
using PulsePanel.Services; // DI container
using Pulse_Panel; // MainWindow is in this namespace

namespace PulsePanel
{
    public partial class App : Application
    {
        public static Window? MainWindow { get; private set; }

        public App()
        {
            this.InitializeComponent();

            // Configure DI before creating MainWindow
            ServiceLocator.ConfigureServices();

            MainWindow = new MainWindow();
            MainWindow.Activate();
        }
    }
}