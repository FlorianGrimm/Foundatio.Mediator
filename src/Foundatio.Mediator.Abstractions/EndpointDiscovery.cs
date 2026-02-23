namespace Foundatio.Mediator;

/// <summary>
/// Controls how handlers are discovered for endpoint generation.
/// </summary>
public enum EndpointDiscovery
{
    /// <summary>
    /// No endpoints are generated.
    /// </summary>
    None,

    /// <summary>
    /// Only handlers explicitly marked with <see cref="HandlerEndpointAttribute"/> generate endpoints.
    /// </summary>
    Explicit,

    /// <summary>
    /// All discovered handlers generate endpoints unless explicitly excluded.
    /// </summary>
    All
}
