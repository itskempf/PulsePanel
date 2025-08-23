using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;
using PulsePanel.App.Services; // Added
using System.Diagnostics; // Added for Debug.WriteLine

namespace PulsePanel.App.Views
{
    public sealed partial class DevTestPage : Page, INotifyPropertyChanged
    {
        private readonly BlueprintLoader _loader; // Added

        private bool _conditionA;
        public bool ConditionA
        {
            get => _conditionA;
            set { _conditionA = value; OnPropertyChanged(nameof(ConditionA)); }
        }

        private bool _conditionB;
        public bool ConditionB
        {
            get => _conditionB;
            set { _conditionB = value; OnPropertyChanged(nameof(ConditionB)); }
        }

        public DevTestPage()
        {
            this.InitializeComponent();
            DataContext = this;
            _loader = App.Current.Services.GetService(typeof(BlueprintLoader)) as BlueprintLoader; // Modified
            LoadBlueprints(); // Added
        }

        private void LoadBlueprints() // Added method
        {
            var bps = _loader.LoadAll();
            foreach (var bp in bps)
            {
                Debug.WriteLine($"Loaded: {bp.Name} ({bp.Version})");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}