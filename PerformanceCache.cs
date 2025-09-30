using System.Collections.Concurrent;

namespace PulsePanel
{
    public static class PerformanceCache
    {
        private static readonly ConcurrentDictionary<string, CacheItem> Cache = new();
        private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(5);

        public static T? Get<T>(string key) where T : class
        {
            if (Cache.TryGetValue(key, out var item) && !item.IsExpired)
                return item.Value as T;
            
            Cache.TryRemove(key, out _);
            return null;
        }

        public static void Set<T>(string key, T value, TimeSpan? expiry = null) where T : class
        {
            Cache[key] = new CacheItem(value, expiry ?? DefaultExpiry);
        }

        public static void Remove(string key)
        {
            Cache.TryRemove(key, out _);
        }

        public static void Clear()
        {
            Cache.Clear();
        }

        private class CacheItem
        {
            public object Value { get; }
            public DateTime ExpiryTime { get; }
            public bool IsExpired => DateTime.Now > ExpiryTime;

            public CacheItem(object value, TimeSpan expiry)
            {
                Value = value;
                ExpiryTime = DateTime.Now.Add(expiry);
            }
        }
    }
}