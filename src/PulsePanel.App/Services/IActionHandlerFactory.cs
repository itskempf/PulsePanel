namespace PulsePanel.App.Services
{
    public interface IActionHandlerFactory
    {
        IActionHandler GetHandler(string actionName);
    }
}