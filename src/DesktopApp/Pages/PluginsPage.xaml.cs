using Microsoft.UI.Xaml.Controls;
using PulsePanel.Core.Services;
using System.Collections.Generic;
using System.Linq;
using PulsePanel.Core.Models;

namespace PulsePanel.DesktopApp.Pages
{
    public sealed partial class PluginsPage : Page
    {
        private PluginManager _pluginManager;
        private List<Plugin> _allPlugins;

        public PluginsPage()
        {
            this.InitializeComponent();
            // Initialize PluginManager (needs a logger and plugins root path)
            // For now, use a dummy logger and a placeholder path
            var logger = new ProvenanceLogger("d:\\PulsePanel\\data\\provenance\\log.jsonl");
            var pluginsRoot = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "plugins");
            _pluginManager = new PluginManager(logger, pluginsRoot);

            LoadPlugins();
        }

        private void LoadPlugins()
        {
            _allPlugins = _pluginManager.LoadPlugins();
            PluginListView.ItemsSource = _allPlugins;
        }

        private void ReloadPlugins_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LoadPlugins();
        }

        private void PluginListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (PluginListView.SelectedItem is Plugin selectedPlugin)
            {
                PluginName.Text = selectedPlugin.Name;
                PluginVersion.Text = selectedPlugin.Version;
                PluginLicense.Text = $"License: {selectedPlugin.License}";
                PluginSha256Hash.Text = $"SHA256: {selectedPlugin.Sha256Hash}";
            }
            else
            {
                // Clear details if nothing is selected
                PluginName.Text = string.Empty;
                PluginVersion.Text = string.Empty;
                PluginLicense.Text = string.Empty;
                PluginSha256Hash.Text = string.Empty;
            }
        }
    }
}
