namespace PulsePanel
{
    public static class RetryHelper
    {
        public static async Task<T> RetryAsync<T>(Func<Task<T>> operation, int maxRetries = 3, TimeSpan? delay = null)
        {
            var retryDelay = delay ?? TimeSpan.FromSeconds(1);
            Exception? lastException = null;

            for (int i = 0; i <= maxRetries; i++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    if (i == maxRetries) break;
                    
                    Logger.LogWarning($"Operation failed (attempt {i + 1}/{maxRetries + 1}): {ex.Message}");
                    await Task.Delay(retryDelay);
                    retryDelay = TimeSpan.FromMilliseconds(retryDelay.TotalMilliseconds * 1.5); // Exponential backoff
                }
            }

            throw lastException ?? new Exception("Operation failed after retries");
        }

        public static async Task RetryAsync(Func<Task> operation, int maxRetries = 3, TimeSpan? delay = null)
        {
            await RetryAsync(async () =>
            {
                await operation();
                return true;
            }, maxRetries, delay);
        }
    }
}