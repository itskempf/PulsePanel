using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace PulsePanel
{
    public class LoadingIndicator : UserControl
    {
        private readonly Grid _grid;
        private readonly Ellipse _spinner;
        private readonly TextBlock _text;
        private readonly Storyboard _animation;

        public LoadingIndicator()
        {
            _grid = new Grid
            {
                Background = new SolidColorBrush(Color.FromArgb(128, 0, 0, 0)),
                Visibility = Visibility.Collapsed
            };

            _spinner = new Ellipse
            {
                Width = 40,
                Height = 40,
                Stroke = new SolidColorBrush(Color.FromRgb(74, 144, 226)),
                StrokeThickness = 4,
                StrokeDashArray = new DoubleCollection { 15, 15 },
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, -20, 0, 0)
            };

            _text = new TextBlock
            {
                Text = "Loading...",
                Foreground = Brushes.White,
                FontSize = 14,
                FontWeight = FontWeights.SemiBold,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(0, 20, 0, 0)
            };

            _grid.Children.Add(_spinner);
            _grid.Children.Add(_text);
            Content = _grid;

            var rotateTransform = new RotateTransform();
            _spinner.RenderTransform = rotateTransform;
            _spinner.RenderTransformOrigin = new Point(0.5, 0.5);

            _animation = new Storyboard();
            var rotation = new DoubleAnimation(0, 360, TimeSpan.FromSeconds(1))
            {
                RepeatBehavior = RepeatBehavior.Forever
            };
            Storyboard.SetTarget(rotation, rotateTransform);
            Storyboard.SetTargetProperty(rotation, new PropertyPath(RotateTransform.AngleProperty));
            _animation.Children.Add(rotation);
        }

        public void Show(string message = "Loading...")
        {
            _text.Text = message;
            _grid.Visibility = Visibility.Visible;
            _animation.Begin();
        }

        public void Hide()
        {
            _grid.Visibility = Visibility.Collapsed;
            _animation.Stop();
        }
    }
}