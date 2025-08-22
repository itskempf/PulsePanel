using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using PulsePanel.App.Services;
using PulsePanel.App.Pages;
using PulsePanel.App.State;

namespace PulsePanel.App
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private object _selectedServer;

    // Navigation
    private readonly INavigationService _navigationService;
    public ICommand NavigateServersCommand { get; }
    public ICommand NavigateBlueprintsCommand { get; }
    public ICommand NavigatePluginsCommand { get; }
    public ICommand NavigateAuditLogCommand { get; }
    public ICommand NavigateSettingsCommand { get; }
    private readonly AppState _state;

        public object SelectedServer
        {
            get => _selectedServer;
            set
            {
                _selectedServer = value;
                OnPropertyChanged();
                // Mirror selection if it’s a ServerEntry
                if (value is PulsePanel.Core.Models.ServerEntry se)
                {
                    _state.SelectedServer = se;
                }
            }
        }

        public MainViewModel() : this(null, null) { }

        public MainViewModel(INavigationService navigationService, AppState state)
        {
            _navigationService = navigationService;
            _state = state ?? App.Services.GetService(typeof(AppState)) as AppState;
            if (_state != null)
            {
                _state.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == nameof(AppState.SelectedServer))
                    {
                        SelectedServer = _state.SelectedServer;
                    }
                };
                SelectedServer = _state.SelectedServer;
            }

            // Commands
            NavigateServersCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(ServersPage)));
            NavigateBlueprintsCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(BlueprintsPage)));
            NavigatePluginsCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(PluginsPage)));
            NavigateAuditLogCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(AuditLogPage)));
            NavigateSettingsCommand = new RelayCommand(_ => _navigationService?.Navigate(typeof(SettingsPage)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    // Server view data is now provided by AppState (Core models)
}
