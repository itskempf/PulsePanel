using Microsoft.UI.Xaml.Controls;
using PulsePanel.App.ViewModels;

namespace PulsePanel.App.Views
{
    public sealed partial class BlueprintCatalogPage : Page
    {
        public BlueprintCatalogViewModel ViewModel { get; }

        public BlueprintCatalogPage()
        {
            this.InitializeComponent();
            ViewModel = App.Current.Services.GetService(typeof(BlueprintCatalogViewModel)) as BlueprintCatalogViewModel;
        }
    }
}