using System;
using System.Collections.Generic;

namespace PulsePanel.App.Services
{
    public class ActionHandlerFactory : IActionHandlerFactory
    {
        private readonly Dictionary<string, IActionHandler> _handlers;

        public ActionHandlerFactory()
        {
            _handlers = new(StringComparer.OrdinalIgnoreCase)
            {
                { "DownloadFile", new DownloadFileActionHandler() }
                // Add more handlers here
            };
        }

        public IActionHandler GetHandler(string actionName)
        {
            if (_handlers.TryGetValue(actionName, out var handler))
                return handler;

            throw new NotSupportedException($"No handler registered for action '{actionName}'");
        }
    }
}