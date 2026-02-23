using System.Collections.Concurrent;
using System.Reflection;
using Foundatio.Mediator;
using Microsoft.Extensions.Logging;

namespace Common.Module.Middleware;

/// <summary>
/// Execute middleware that caches handler results keyed by message value.
/// Only applies to handlers decorated with <see cref="CachedAttribute"/> (ExplicitOnly = true).
/// Because C# records use value equality, identical query messages produce cache hits automatically.
/// </summary>
[Middleware(Order = 100, ExplicitOnly = true)]
public static class CachingMiddleware
{
    private static readonly ConcurrentDictionary<object, CacheEntry> Cache = new();
    private static readonly ConcurrentDictionary<MethodInfo, CacheSettings> SettingsCache = new();

    public static async ValueTask<object?> ExecuteAsync(
        object message,
        HandlerExecutionDelegate next,
        HandlerExecutionInfo handlerInfo,
        ILogger<IMediator> logger)
    {
        var settings = SettingsCache.GetOrAdd(handlerInfo.HandlerMethod, method =>
        {
            var attr = method.GetCustomAttribute<CachedAttribute>();
            return new CacheSettings
            {
                Duration = TimeSpan.FromSeconds(attr?.DurationSeconds ?? 300),
                SlidingExpiration = attr?.SlidingExpiration ?? false
            };
        });

        // Check for a valid cached entry
        if (Cache.TryGetValue(message, out var entry))
        {
            if (!entry.IsExpired)
            {
                logger.LogDebug("CachingMiddleware: Cache HIT for {MessageType}", message.GetType().Name);
                if (settings.SlidingExpiration)
                    entry.LastAccessed = DateTime.UtcNow;
                return entry.Value;
            }

            Cache.TryRemove(message, out _);
            logger.LogDebug("CachingMiddleware: Cache EXPIRED for {MessageType}", message.GetType().Name);
        }

        // Cache miss — execute the full pipeline
        logger.LogDebug("CachingMiddleware: Cache MISS for {MessageType}, executing handler", message.GetType().Name);
        var result = await next();

        Cache[message] = new CacheEntry
        {
            Value = result,
            CreatedAt = DateTime.UtcNow,
            LastAccessed = DateTime.UtcNow,
            Duration = settings.Duration,
            SlidingExpiration = settings.SlidingExpiration
        };

        if (Cache.Count > 1000)
            CleanupExpiredEntries();

        return result;
    }

    /// <summary>Removes a specific message's cached result.</summary>
    public static void Invalidate(object message) => Cache.TryRemove(message, out _);

    /// <summary>Clears the entire cache.</summary>
    public static void Clear() => Cache.Clear();

    private static void CleanupExpiredEntries()
    {
        var expiredKeys = Cache.Where(kvp => kvp.Value.IsExpired).Select(kvp => kvp.Key).ToList();
        foreach (var key in expiredKeys)
            Cache.TryRemove(key, out _);
    }

    private sealed class CacheEntry
    {
        public object? Value { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime LastAccessed { get; set; }
        public TimeSpan Duration { get; init; }
        public bool SlidingExpiration { get; init; }

        public bool IsExpired => SlidingExpiration
            ? DateTime.UtcNow - LastAccessed > Duration
            : DateTime.UtcNow - CreatedAt > Duration;
    }

    private sealed class CacheSettings
    {
        public TimeSpan Duration { get; init; }
        public bool SlidingExpiration { get; init; }
    }
}
