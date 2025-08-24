using System;

namespace PulsePanel.Services;

public class NavigationService
{
    public event Action<Type>? NavigateRequested;

    public void RequestNavigate(Type pageType)
    {
        NavigateRequested?.Invoke(pageType);
    }
}
