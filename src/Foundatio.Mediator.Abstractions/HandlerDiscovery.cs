namespace Foundatio.Mediator;

/// <summary>
/// Controls how handlers are discovered by the source generator.
/// </summary>
public enum HandlerDiscovery
{
    /// <summary>
    /// Handlers are discovered by naming convention (suffix: Handler, Consumer)
    /// and explicit declaration (<see cref="HandlerAttribute"/> or <see cref="IHandler"/>).
    /// This is the default.
    /// </summary>
    All = 0,

    /// <summary>
    /// Only handlers explicitly marked with <see cref="HandlerAttribute"/>
    /// or implementing <see cref="IHandler"/> are discovered.
    /// Naming convention discovery is disabled.
    /// </summary>
    Explicit = 1
}
