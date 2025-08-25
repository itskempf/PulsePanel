
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PulsePanel.ViewModels
{
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        protected void SetProperty<T>(ref T field, T value, [CallerMemberName] string? name = null)
        {
            if (!Equals(field, value)) { field = value; PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name)); }
        }
    }

    public sealed class RelayCommand : ICommand
    {
        private readonly Action _run; private readonly Func<bool>? _can;
        public RelayCommand(Action run, Func<bool>? can = null) { _run = run; _can = can; }
        public bool CanExecute(object? p) => _can?.Invoke() ?? true; public void Execute(object? p) => _run();
        public event EventHandler? CanExecuteChanged; public void RaiseCanExecuteChanged()=> CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public sealed class RelayCommand<T> : ICommand
    {
        private readonly Action<T?> _run; private readonly Func<T?, bool>? _can;
        public RelayCommand(Action<T?> run, Func<T?, bool>? can = null) { _run = run; _can = can; }
        public bool CanExecute(object? p) => _can?.Invoke((T?)p) ?? true; public void Execute(object? p) => _run((T?)p);
        public event EventHandler? CanExecuteChanged; public void RaiseCanExecuteChanged()=> CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public sealed class AsyncCommand : ICommand
    {
        private readonly Func<Task> _run; private readonly Func<bool>? _can; private bool _busy;
        public AsyncCommand(Func<Task> run, Func<bool>? can = null) { _run = run; _can = can; }
        public bool CanExecute(object? p) => !_busy && (_can?.Invoke() ?? true);
        public async void Execute(object? p) { _busy = true; RaiseCanExecuteChanged(); try { await _run(); } finally { _busy = false; RaiseCanExecuteChanged(); } }
        public event EventHandler? CanExecuteChanged; public void RaiseCanExecuteChanged()=> CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
    public sealed class AsyncCommand<T> : ICommand
    {
        private readonly Func<T?, Task> _run; private readonly Func<T?, bool>? _can; private bool _busy;
        public AsyncCommand(Func<T?, Task> run, Func<T?, bool>? can = null) { _run = run; _can = can; }
        public bool CanExecute(object? p) => !_busy && (_can?.Invoke((T?)p) ?? true);
        public async void Execute(object? p) { _busy = true; RaiseCanExecuteChanged(); try { await _run((T?)p); } finally { _busy = false; RaiseCanExecuteChanged(); } }
        public event EventHandler? CanExecuteChanged; public void RaiseCanExecuteChanged()=> CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}
