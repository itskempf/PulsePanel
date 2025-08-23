using Microsoft.UI.Xaml.Controls;

namespace PulsePanel.App.Views
{
    public sealed partial class JobQueuePage : Page
    {
        public JobQueueViewModel ViewModel => (JobQueueViewModel)DataContext;
        public JobQueuePage() { InitializeComponent(); }
    }
}