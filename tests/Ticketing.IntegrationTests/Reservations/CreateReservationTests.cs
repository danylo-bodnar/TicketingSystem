using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Events;
using Ticketing.Domain.Events;

public class CreateReservationTests : IntegrationTestBase
{
    public CreateReservationTests(TestDatabaseFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task CreateReservation_ShouldWriteOutboxMessage_WithCorrectPayload()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var seatId = screening.Seats.First().SeatId;

        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });

        response.EnsureSuccessStatusCode();

        using var verifyDb = CreateDbContext();
        var message = await verifyDb.OutboxMessages.SingleAsync();

        Assert.Equal("ReservationCreated", message.Type);
        Assert.Contains(seatId.ToString(), message.Payload);
        Assert.Contains(screening.Id.ToString(), message.Payload);
    }

    [Fact]
    public async Task ShouldNotAllowDuplicateSeatReservation()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var seatId = screening.Seats.First().SeatId;

        var r1 = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });

        r1.EnsureSuccessStatusCode();

        var r2 = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });

        Assert.False(r2.IsSuccessStatusCode);

        using var verifyDb = CreateDbContext();
        var messages = await verifyDb.OutboxMessages.ToListAsync();
        Assert.Single(messages);
    }

    [Fact]
    public async Task ShouldNotWriteOutboxMessage_WhenReservationFails()
    {
        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = Guid.NewGuid(),
            seatIds = new[] { Guid.NewGuid() }
        });

        Assert.False(response.IsSuccessStatusCode);

        using var db = CreateDbContext();
        var messages = await db.OutboxMessages.ToListAsync();

        Assert.Empty(messages);
    }

    [Fact]
    public async Task Reservation_ShouldCreatePayment_ThroughOutboxFlow()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var seatId = screening.Seats.First().SeatId;

        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });

        response.EnsureSuccessStatusCode();

        await RunOutboxProcessorOnce();

        using var verify = CreateDbContext();
        var payments = await verify.Payments.ToListAsync();

        Assert.Single(payments);
    }

    [Fact]
    public async Task OutboxHandler_ShouldBeIdempotent()
    {
        var reservationId = Guid.NewGuid();
        var handler = GetService<IEventHandler<ReservationCreated>>();

        var evt = new ReservationCreated(
            reservationId,
            Guid.NewGuid(),
            new List<Guid> { Guid.NewGuid() },
            DateTime.UtcNow
        );

        await handler.HandleAsync(evt);
        await handler.HandleAsync(evt);

        using var db = CreateDbContext();
        var payments = await db.Payments
            .Where(p => p.ReservationId == reservationId)
            .ToListAsync();

        Assert.Single(payments);
    }

    [Fact]
    public async Task OnlyOneReservation_ShouldSucceed_ForSameSeat_UnderConcurrency()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var seatId = screening.Seats.First().SeatId;

        var tasks = Enumerable.Range(0, 50).Select(_ =>
            Client.PostAsJsonAsync("/api/Reservations", new
            {
                screeningId = screening.Id,
                seatIds = new[] { seatId }
            })
        );

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.IsSuccessStatusCode);
        Assert.Equal(1, successCount);

        using var verifyDb = CreateDbContext();

        var reservations = await verifyDb.Reservations.ToListAsync();
        var outbox = await verifyDb.OutboxMessages.ToListAsync();

        Assert.Single(reservations);
        Assert.Single(outbox);
    }

    [Fact]
    public async Task ShouldFail_WhenSeatListIsEmpty()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.FirstAsync();

        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = Array.Empty<Guid>()
        });

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task ShouldFail_WhenDuplicateSeatIdsProvided()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var seatId = screening.Seats.First().SeatId;

        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId, seatId }
        });

        Assert.False(response.IsSuccessStatusCode);
    }
}
