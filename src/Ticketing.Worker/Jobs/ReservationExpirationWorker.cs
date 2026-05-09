using Ticketing.Application.Reservations.Services;

public class ReservationExpirationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservationExpirationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var service = scope.ServiceProvider
                .GetRequiredService<ReservationExpirationService>();

            await service.ProcessOnce(ct);
            await Task.Delay(TimeSpan.FromSeconds(30), ct);
        }
    }
}