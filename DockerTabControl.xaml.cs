using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PulsePanel
{
    public partial class DockerTabControl : UserControl
    {
        private readonly DockerManager _dockerManager;
        private List<DockerContainer> _containers;

        public DockerTabControl()
        {
            InitializeComponent();
            _dockerManager = new DockerManager();
            _containers = new List<DockerContainer>();
            
            Loaded += async (s, e) => await InitializeDocker();
        }

        private async Task InitializeDocker()
        {
            var isAvailable = await _dockerManager.IsDockerAvailable();
            
            if (isAvailable)
            {
                DockerStatusIcon.Text = "🐳";
                DockerStatusText.Text = "Docker Status: Available";
                DockerStatusText.Foreground = System.Windows.Media.Brushes.Green;
                await RefreshContainers();
            }
            else
            {
                DockerStatusIcon.Text = "❌";
                DockerStatusText.Text = "Docker Status: Not Available";
                DockerStatusText.Foreground = System.Windows.Media.Brushes.Red;
                CreateContainerBtn.IsEnabled = false;
            }
        }

        private async void RefreshContainers_Click(object sender, RoutedEventArgs e)
        {
            await RefreshContainers();
        }

        private async Task RefreshContainers()
        {
            try
            {
                _containers = await _dockerManager.GetContainers();
                ContainersGrid.ItemsSource = _containers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to refresh containers: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CreateContainer_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new DockerCreateDialog();
            if (dialog.ShowDialog() == true)
            {
                _ = Task.Run(async () =>
                {
                    await CreateContainerFromTemplate(dialog.SelectedTemplate, dialog.ContainerName, dialog.CustomSettings);
                    await Dispatcher.InvokeAsync(RefreshContainers);
                });
            }
        }

        private async Task CreateContainerFromTemplate(DockerGameTemplate template, string name, Dictionary<string, string> customSettings)
        {
            try
            {
                var ports = new Dictionary<string, string>(template.DefaultPorts);
                var environment = new Dictionary<string, string>(template.DefaultEnvironment);
                var volumes = new Dictionary<string, string>
                {
                    [$"pulsepanel_{name}_data"] = template.ConfigPath
                };

                // Apply custom settings
                foreach (var setting in customSettings)
                {
                    if (setting.Key.StartsWith("PORT_"))
                    {
                        var portKey = setting.Key.Substring(5);
                        if (ports.ContainsKey(portKey))
                            ports[portKey] = setting.Value;
                    }
                    else
                    {
                        environment[setting.Key] = setting.Value;
                    }
                }

                var success = await _dockerManager.CreateContainer(name, template.Image, ports, volumes, environment);
                
                if (success)
                {
                    await Dispatcher.InvokeAsync(() =>
                        MessageBox.Show($"Container '{name}' created successfully!", "Success", 
                            MessageBoxButton.OK, MessageBoxImage.Information));
                }
                else
                {
                    await Dispatcher.InvokeAsync(() =>
                        MessageBox.Show($"Failed to create container '{name}'", "Error", 
                            MessageBoxButton.OK, MessageBoxImage.Error));
                }
            }
            catch (Exception ex)
            {
                await Dispatcher.InvokeAsync(() =>
                    MessageBox.Show($"Error creating container: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error));
            }
        }

        private async void StartContainer_Click(object sender, RoutedEventArgs e)
        {
            if (ContainersGrid.SelectedItem is DockerContainer container)
            {
                await _dockerManager.StartContainer(container.Name);
                await RefreshContainers();
            }
        }

        private async void StopContainer_Click(object sender, RoutedEventArgs e)
        {
            if (ContainersGrid.SelectedItem is DockerContainer container)
            {
                await _dockerManager.StopContainer(container.Name);
                await RefreshContainers();
            }
        }

        private async void ViewLogs_Click(object sender, RoutedEventArgs e)
        {
            if (ContainersGrid.SelectedItem is DockerContainer container)
            {
                var logs = await _dockerManager.GetContainerLogs(container.Name);
                var logWindow = new Window
                {
                    Title = $"Logs - {container.Name}",
                    Width = 800,
                    Height = 600,
                    Content = new ScrollViewer
                    {
                        Content = new TextBox
                        {
                            Text = logs,
                            IsReadOnly = true,
                            FontFamily = new System.Windows.Media.FontFamily("Consolas"),
                            Background = System.Windows.Media.Brushes.Black,
                            Foreground = System.Windows.Media.Brushes.White,
                            Padding = new Thickness(10)
                        }
                    }
                };
                logWindow.Show();
            }
        }

        private async void RemoveContainer_Click(object sender, RoutedEventArgs e)
        {
            if (ContainersGrid.SelectedItem is DockerContainer container)
            {
                var result = MessageBox.Show($"Remove container '{container.Name}'?", "Confirm", 
                    MessageBoxButton.YesNo, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Yes)
                {
                    await _dockerManager.RemoveContainer(container.Name);
                    await RefreshContainers();
                }
            }
        }

        private void ContainersGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Handle selection changes if needed
        }
    }
}