using Microsoft.EntityFrameworkCore;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Domain.Reservations;

public class ReservationExpirationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReservationExpirationWorker(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

            var expiredReservations = await db.Reservations
                .Where(r => r.Status == ReservationStatus.Pending &&
                            r.ExpiredAt <= DateTime.UtcNow)
                .ToListAsync(stoppingToken);

            foreach (var reservation in expiredReservations)
            {
                reservation.Expire();

                foreach (var seatId in reservation.SeatIds)
                {
                    var seats = await db.ScreeningSeats
                    .Where(s =>
                        s.ScreeningId == reservation.ScreeningId &&
                        reservation.SeatIds.Contains(s.SeatId))
                    .ToListAsync(stoppingToken);

                    foreach (var seat in seats)
                    {
                        seat.Release();
                    }
                }
            }

            await db.SaveChangesAsync(stoppingToken);

            await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
        }
    }
}