using Common.Module.Messages;
using Foundatio.Mediator;
using Microsoft.AspNetCore.Authorization;

namespace Common.Module.Handlers;

/// <summary>
/// A public health check endpoint that demonstrates using <c>[AllowAnonymous]</c>
/// to opt a handler out of the global <c>RequireAuth = true</c> setting.
///
/// Even though all generated endpoints require authentication by default, this
/// handler is accessible without logging in — useful for load balancers, uptime
/// monitors, and readiness probes.
/// </summary>
[AllowAnonymous]
[HandlerCategory("Health")]
public class HealthHandler
{
    public HealthStatusResponse Handle(GetHealthStatus query) =>
        new("Healthy", "1.0.0", DateTime.UtcNow);
}
