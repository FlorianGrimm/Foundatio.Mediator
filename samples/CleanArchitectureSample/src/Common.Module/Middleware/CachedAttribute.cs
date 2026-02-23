using Foundatio.Mediator;

namespace Common.Module.Middleware;

/// <summary>
/// Caches handler results so repeated calls with the same message return instantly.
/// Apply to query handler methods to avoid redundant work on repeated identical requests.
/// </summary>
/// <example>
/// <code>
/// // Fixed expiration (default 5 minutes)
/// [Cached]
/// public Result&lt;Product&gt; Handle(GetProduct query) { ... }
///
/// // Custom duration with sliding expiration
/// [Cached(DurationSeconds = 60, SlidingExpiration = true)]
/// public Result&lt;List&lt;Product&gt;&gt; Handle(GetProducts query) { ... }
/// </code>
/// </example>
[UseMiddleware(typeof(CachingMiddleware))]
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
public sealed class CachedAttribute : Attribute
{
    /// <summary>
    /// How long cached results remain valid, in seconds. Default is 300 (5 minutes).
    /// </summary>
    public int DurationSeconds { get; set; } = 300;

    /// <summary>
    /// When true, the expiration timer resets on each cache hit.
    /// When false (default), entries expire at a fixed time after creation.
    /// </summary>
    public bool SlidingExpiration { get; set; }
}
