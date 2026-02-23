using Foundatio.Mediator;
using Microsoft.Extensions.Logging;

using Common.Module.Middleware;

namespace Products.Module.Middleware;

[Middleware(OrderAfter = [typeof(ValidationMiddleware)])]
public static class ProductsModuleMiddleware
{
    public static void Before(object message, ILogger<IMediator> logger)
    {
        logger.LogInformation("ProductsModuleMiddleware Before handling {MessageType}", message.GetType().Name);
    }
}
