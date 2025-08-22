using Microsoft.UI.Xaml.Controls;
using System;

namespace PulsePanel.App.Services
{
    public interface INavigationService
    {
        void Initialize(Frame frame);
        bool Navigate(Type pageType, object parameter = null);
        void GoBack();
    }

    public class NavigationService : INavigationService
    {
        private Frame _frame;

        public void Initialize(Frame frame)
        {
            _frame = frame;
        }

        public bool Navigate(Type pageType, object parameter = null)
        {
            if (_frame == null)
                throw new InvalidOperationException("NavigationService not initialized with a Frame.");

            if (_frame.CurrentSourcePageType != pageType)
                return _frame.Navigate(pageType, parameter);

            return false;
        }

        public void GoBack()
        {
            if (_frame?.CanGoBack == true)
                _frame.GoBack();
        }
    }
}
