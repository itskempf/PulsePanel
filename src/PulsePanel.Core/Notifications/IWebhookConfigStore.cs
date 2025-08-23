namespace PulsePanel.Core.Notifications;

public interface IWebhookConfigStore
{
    Task<IReadOnlyList<WebhookConfig>> LoadAsync(CancellationToken ct = default);
    Task SaveAsync(IEnumerable<WebhookConfig> configs, CancellationToken ct = default);
    IAsyncEnumerable<IReadOnlyList<WebhookConfig>> WatchAsync(CancellationToken ct = default);
}
