using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PulsePanel
{
    public class ToastNotification : Window
    {
        public ToastNotification(string title, string message, bool isError = false)
        {
            InitializeComponent(title, message, isError);
            ShowToast();
        }

        private void InitializeComponent(string title, string message, bool isError)
        {
            Width = 350;
            Height = 100;
            WindowStyle = WindowStyle.None;
            AllowsTransparency = true;
            Background = Brushes.Transparent;
            Topmost = true;
            ShowInTaskbar = false;

            // Position at bottom right
            var workArea = SystemParameters.WorkArea;
            Left = workArea.Right - Width - 20;
            Top = workArea.Bottom - Height - 20;

            var border = new Border
            {
                Background = new SolidColorBrush(isError ? Color.FromRgb(220, 53, 69) : Color.FromRgb(40, 167, 69)),
                CornerRadius = new CornerRadius(8),
                Padding = new Thickness(15),
                Effect = new System.Windows.Media.Effects.DropShadowEffect
                {
                    Color = Colors.Black,
                    Opacity = 0.3,
                    ShadowDepth = 3
                }
            };

            var stackPanel = new StackPanel();
            
            var titleBlock = new TextBlock
            {
                Text = title,
                Foreground = Brushes.White,
                FontWeight = FontWeights.Bold,
                FontSize = 14
            };

            var messageBlock = new TextBlock
            {
                Text = message,
                Foreground = Brushes.White,
                FontSize = 12,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(0, 5, 0, 0)
            };

            stackPanel.Children.Add(titleBlock);
            stackPanel.Children.Add(messageBlock);
            border.Child = stackPanel;
            Content = border;
        }

        private async void ShowToast()
        {
            // Slide in animation
            var slideIn = new DoubleAnimation(Width, 0, TimeSpan.FromMilliseconds(300));
            var transform = new TranslateTransform();
            RenderTransform = transform;
            transform.BeginAnimation(TranslateTransform.XProperty, slideIn);

            Show();

            // Auto-hide after 4 seconds
            await Task.Delay(4000);
            
            var slideOut = new DoubleAnimation(0, Width, TimeSpan.FromMilliseconds(300));
            slideOut.Completed += (s, e) => Close();
            transform.BeginAnimation(TranslateTransform.XProperty, slideOut);
        }
    }
}