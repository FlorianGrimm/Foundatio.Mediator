using Foundatio.Mediator;

[assembly: MediatorConfiguration(
    HandlerLifetime = MediatorLifetime.Scoped,
    MiddlewareLifetime = MediatorLifetime.Scoped
)]
