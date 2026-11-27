using PopPop.Services.Interfaces;

namespace PopPop.Services.Implementations;

/// <summary>
/// Every minute, cancels VNPAY orders the customer did not pay in time so their items return to stock
/// instead of staying reserved forever.
/// </summary>
public sealed class OnlinePaymentExpiryService(
    IServiceScopeFactory scopeFactory,
    ILogger<OnlinePaymentExpiryService> logger) : BackgroundService
{
    private static readonly TimeSpan Interval = TimeSpan.FromMinutes(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(Interval);
        do
        {
            try
            {
                await using var scope = scopeFactory.CreateAsyncScope();
                var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();
                var cancelled = await orderService.CancelExpiredOnlinePaymentsAsync(stoppingToken);
                if (cancelled > 0)
                {
                    logger.LogInformation("Cancelled {Count} unpaid VNPAY order(s) past their payment deadline.", cancelled);
                }
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Could not cancel expired VNPAY orders.");
            }
        }
        while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
