using Microsoft.UI.Xaml.Controls;
using System.ComponentModel;

namespace PulsePanel.Views
{
    public sealed partial class DevTestPage : Page, INotifyPropertyChanged
    {
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
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}