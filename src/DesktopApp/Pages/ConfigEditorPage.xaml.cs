
using Microsoft.UI.Xaml.Controls;
using PulsePanel.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DesktopApp.Pages
{
    public sealed partial class ConfigEditorPage : Page
    {
        private readonly IConfigEditor _configEditor;
        private string _serverId;
        private string _filePath;

        public ConfigEditorPage()
        {
            this.InitializeComponent();
            _configEditor = App.Services.GetService<IConfigEditor>();
        }

        public void LoadConfig(string serverId, string filePath)
        {
            _serverId = serverId;
            _filePath = filePath;
            EditorTextBox.Text = _configEditor.ReadConfigFileAsync(_serverId, _filePath).Result;
        }

        private void SaveButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            _configEditor.WriteConfigFileAsync(_serverId, _filePath, EditorTextBox.Text);
        }
    }
}
