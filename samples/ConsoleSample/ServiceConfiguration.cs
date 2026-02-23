using Foundatio.Mediator;
using Foundatio.Resilience;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ConsoleSample;

public static class ServiceConfiguration
{
    public static IServiceCollection ConfigureServices(this IServiceCollection services)
    {
        // Add logging
        services.AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Information));

        // Register named resilience policies
        services.AddSingleton<IResiliencePolicyProvider>(
            new ResiliencePolicyProvider()
                .WithPolicy("aggressive", p => p
                    .WithMaxAttempts(10)
                    .WithExponentialDelay(TimeSpan.FromMilliseconds(50))
                    .WithJitter()));

        // Add Foundatio Mediator
        services.AddMediator();

        return services;
    }
}
