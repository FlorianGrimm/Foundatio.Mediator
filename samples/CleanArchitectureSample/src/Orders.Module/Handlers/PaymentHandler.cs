using Common.Module.Middleware;
using Foundatio.Mediator;
using Microsoft.Extensions.Logging;
using Orders.Module.Messages;

namespace Orders.Module.Handlers;

/// <summary>
/// Simulates a payment gateway that experiences transient failures.
/// Demonstrates how the [Retry] middleware automatically retries on exceptions.
/// Approximately 60% of first attempts will fail, but retries will succeed.
/// </summary>
public class PaymentHandler
{
    private static int _attemptCount;

    /// <summary>
    /// Processes a payment, randomly throwing transient errors to demonstrate retry.
    /// </summary>
    [Retry(MaxAttempts = 5, DelayMs = 100)]
    public Task<Result<string>> HandleAsync(
        ProcessPayment command,
        ILogger<PaymentHandler> logger,
        CancellationToken cancellationToken)
    {
        var attempt = Interlocked.Increment(ref _attemptCount);

        // Simulate transient failures ~60% of the time
        if (Random.Shared.NextDouble() < 0.6)
        {
            logger.LogWarning(
                "Payment attempt #{Attempt} for order {OrderId} failed — transient gateway error",
                attempt, command.OrderId);

            throw new InvalidOperationException(
                $"Transient payment gateway error (attempt #{attempt})");
        }

        var confirmationId = $"PAY-{Guid.NewGuid():N}"[..16].ToUpperInvariant();

        logger.LogInformation(
            "Payment of {Amount:C} for order {OrderId} succeeded on attempt #{Attempt} — confirmation {ConfirmationId}",
            command.Amount, command.OrderId, attempt, confirmationId);

        return Task.FromResult<Result<string>>(confirmationId);
    }
}
