
using Microsoft.Extensions.DependencyInjection;

namespace PulsePanel.Services
{
    public static class ServiceLocator
    {
        public static T Get<T>() where T : notnull => PulsePanel.App.Services.GetRequiredService<T>();
    }
}
