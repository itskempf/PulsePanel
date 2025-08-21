using Microsoft.UI.Xaml.Controls;
using PulsePanel.DesktopApp.Services;
using PulsePanel.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace PulsePanel.DesktopApp.Pages
{
    public sealed partial class BlueprintsPage : Page
    {
        private List<BlueprintCatalogEntry> _allBlueprints;

        public BlueprintsPage()
        {
            this.InitializeComponent();
            var dataService = new DataService();
            _allBlueprints = dataService.GetBlueprints();
            BlueprintListView.ItemsSource = _allBlueprints;
        }

        private void BlueprintListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (BlueprintListView.SelectedItem is BlueprintCatalogEntry selectedBlueprint)
            {
                BlueprintName.Text = selectedBlueprint.Name;
                BlueprintVersion.Text = selectedBlueprint.Version;
                BlueprintDescription.Text = selectedBlueprint.Description;
                BlueprintAuthor.Text = $"Author: {selectedBlueprint.Author}";
                BlueprintLicense.Text = $"License: {selectedBlueprint.License}";
                BlueprintSourceUrl.Text = $"Source URL: {selectedBlueprint.SourceUrl}";
                BlueprintSourceHash.Text = $"Source Hash: {selectedBlueprint.SourceHash}";
            }
            else
            {
                // Clear the metadata panel if nothing is selected
                BlueprintName.Text = string.Empty;
                BlueprintVersion.Text = string.Empty;
                BlueprintDescription.Text = string.Empty;
                BlueprintAuthor.Text = string.Empty;
                BlueprintLicense.Text = string.Empty;
                BlueprintSourceUrl.Text = string.Empty;
                BlueprintSourceHash.Text = string.Empty;
            }
        }

        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            var query = SearchBox.Text.ToLowerInvariant();
            var filteredBlueprints = _allBlueprints.Where(b =>
                b.Name.ToLowerInvariant().Contains(query) ||
                b.Description.ToLowerInvariant().Contains(query) ||
                b.Author.ToLowerInvariant().Contains(query)
            ).ToList();
            BlueprintListView.ItemsSource = filteredBlueprints;
        }
    }
}
