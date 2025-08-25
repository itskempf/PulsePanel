
using System.Threading.Tasks;

namespace PulsePanel.ViewModels
{
    public static class CommandExtensions
    {
        public static Task ExecuteAsync(this AsyncCommand cmd) => Task.Run(() => cmd.Execute(null));
    }
}
