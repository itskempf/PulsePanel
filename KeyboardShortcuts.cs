using System.Windows;
using System.Windows.Input;

namespace PulsePanel
{
    public static class KeyboardShortcuts
    {
        public static void RegisterShortcuts(Window window, MainWindow mainWindow)
        {
            // Ctrl+N - Add Server
            var addServerCommand = new RoutedCommand();
            addServerCommand.InputGestures.Add(new KeyGesture(Key.N, ModifierKeys.Control));
            window.CommandBindings.Add(new CommandBinding(addServerCommand, (s, e) => mainWindow.AddServer_Click(s, null)));

            // Ctrl+R - Scan Servers
            var scanCommand = new RoutedCommand();
            scanCommand.InputGestures.Add(new KeyGesture(Key.R, ModifierKeys.Control));
            window.CommandBindings.Add(new CommandBinding(scanCommand, (s, e) => mainWindow.ScanServers_Click(s, null)));

            // F5 - Start Server
            var startCommand = new RoutedCommand();
            startCommand.InputGestures.Add(new KeyGesture(Key.F5));
            window.CommandBindings.Add(new CommandBinding(startCommand, (s, e) => mainWindow.StartServer_Click(s, null)));

            // F6 - Stop Server
            var stopCommand = new RoutedCommand();
            stopCommand.InputGestures.Add(new KeyGesture(Key.F6));
            window.CommandBindings.Add(new CommandBinding(stopCommand, (s, e) => mainWindow.StopServer_Click(s, null)));

            // F7 - Restart Server
            var restartCommand = new RoutedCommand();
            restartCommand.InputGestures.Add(new KeyGesture(Key.F7));
            window.CommandBindings.Add(new CommandBinding(restartCommand, (s, e) => mainWindow.RestartServer_Click(s, null)));

            // Ctrl+U - Update Server
            var updateCommand = new RoutedCommand();
            updateCommand.InputGestures.Add(new KeyGesture(Key.U, ModifierKeys.Control));
            window.CommandBindings.Add(new CommandBinding(updateCommand, (s, e) => mainWindow.UpdateServer_Click(s, null)));

            // Ctrl+, - Settings
            var settingsCommand = new RoutedCommand();
            settingsCommand.InputGestures.Add(new KeyGesture(Key.OemComma, ModifierKeys.Control));
            window.CommandBindings.Add(new CommandBinding(settingsCommand, (s, e) => mainWindow.Settings_Click(s, null)));

            // Tab navigation (1-4)
            for (int i = 1; i <= 4; i++)
            {
                var tabCommand = new RoutedCommand();
                var key = (Key)Enum.Parse(typeof(Key), $"D{i}");
                tabCommand.InputGestures.Add(new KeyGesture(key, ModifierKeys.Control));
                var tabName = i switch { 1 => "Status", 2 => "Config", 3 => "Mods", 4 => "Schedule", _ => "Status" };
                window.CommandBindings.Add(new CommandBinding(tabCommand, (s, e) => mainWindow.SwitchTab(tabName)));
            }
        }
    }
}