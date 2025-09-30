using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class DockerCreateDialog : Window
    {
        public DockerGameTemplate SelectedTemplate { get; private set; }
        public string ContainerName { get; private set; }
        public Dictionary<string, string> CustomSettings { get; private set; }

        public DockerCreateDialog()
        {
            InitializeComponent();
            CustomSettings = new Dictionary<string, string>();
            LoadGameTypes();
        }

        private void LoadGameTypes()
        {
            GameTypeCombo.ItemsSource = DockerGameTemplates.Templates.Select(t => new
            {
                Key = t.Key,
                Name = t.Value.Name
            }).ToList();
            GameTypeCombo.DisplayMemberPath = "Name";
            GameTypeCombo.SelectedValuePath = "Key";
        }

        private void GameTypeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GameTypeCombo.SelectedValue is string key && DockerGameTemplates.Templates.ContainsKey(key))
            {
                SelectedTemplate = DockerGameTemplates.Templates[key];
                DockerImageBox.Text = SelectedTemplate.Image;
                ContainerNameBox.Text = $"{key}-server-{DateTime.Now:yyyyMMdd-HHmmss}";
                LoadSettings();
            }
        }

        private void LoadSettings()
        {
            SettingsPanel.Children.Clear();

            if (SelectedTemplate == null) return;

            // Port settings
            foreach (var port in SelectedTemplate.DefaultPorts)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock
                {
                    Text = $"Port {port.Key}:",
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);

                var textBox = new TextBox
                {
                    Text = port.Value.Replace("/udp", "").Replace("/tcp", ""),
                    Height = 25,
                    Tag = $"PORT_{port.Key}"
                };
                Grid.SetColumn(textBox, 1);

                grid.Children.Add(label);
                grid.Children.Add(textBox);
                SettingsPanel.Children.Add(grid);
            }

            // Environment settings
            foreach (var env in SelectedTemplate.DefaultEnvironment)
            {
                var grid = new Grid { Margin = new Thickness(0, 0, 0, 10) };
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(150) });
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });

                var label = new TextBlock
                {
                    Text = $"{env.Key}:",
                    VerticalAlignment = VerticalAlignment.Center
                };
                Grid.SetColumn(label, 0);

                var textBox = new TextBox
                {
                    Text = env.Value,
                    Height = 25,
                    Tag = env.Key
                };
                Grid.SetColumn(textBox, 1);

                grid.Children.Add(label);
                grid.Children.Add(textBox);
                SettingsPanel.Children.Add(grid);
            }
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ContainerNameBox.Text))
            {
                MessageBox.Show("Please enter a container name.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (SelectedTemplate == null)
            {
                MessageBox.Show("Please select a game type.", "Validation Error", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            ContainerName = ContainerNameBox.Text.Trim();
            CustomSettings.Clear();

            // Collect custom settings
            foreach (Grid grid in SettingsPanel.Children.OfType<Grid>())
            {
                var textBox = grid.Children.OfType<TextBox>().FirstOrDefault();
                if (textBox != null && textBox.Tag is string key)
                {
                    CustomSettings[key] = textBox.Text;
                }
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}