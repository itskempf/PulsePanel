using Microsoft.UI.Xaml.Controls;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App.Views
{
    public sealed partial class BlueprintControlCenterPage : Page
    {
        public BlueprintCatalogViewModel CatalogVM { get; }
        public BlueprintExecutionLogViewModel LogVM { get; }
        public BlueprintHistoryViewModel HistoryVM { get; } // Added

        public BlueprintControlCenterPage(BlueprintCatalogViewModel catalogVM,
                                          BlueprintExecutionLogViewModel logVM,
                                          BlueprintHistoryViewModel historyVM) // Modified constructor
        {
            this.InitializeComponent();
            CatalogVM = catalogVM;
            LogVM = logVM;
            HistoryVM = historyVM; // Added
        }
    }
}