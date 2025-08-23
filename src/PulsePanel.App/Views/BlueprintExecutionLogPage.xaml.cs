using Microsoft.UI.Xaml.Controls;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App.Views
{
    public sealed partial class BlueprintExecutionLogPage : Page
    {
        public BlueprintExecutionLogViewModel ViewModel { get; }

        public BlueprintExecutionLogPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService(typeof(BlueprintExecutionLogViewModel)) as BlueprintExecutionLogViewModel;
        }
    }
}