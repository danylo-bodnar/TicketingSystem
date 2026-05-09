using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Events;
using Ticketing.Contracts.Reservations;
using Ticketing.Domain.Events;
using Ticketing.Domain.Payments;
using Ticketing.Domain.Reservations;
using Ticketing.Domain.Screenings;
using Ticketing.Infrastructure.Seed;

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

        var evt = new ReservationCreated
        {
            ReservationId = reservationId,
            ScreeningId = DataSeeder.Screening1Id,
            SeatIds = new List<Guid> { Guid.NewGuid() },
            CreatedAt = DateTime.UtcNow
        };

        await handler.HandleAsync(evt, CancellationToken.None);
        await handler.HandleAsync(evt, CancellationToken.None);

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
    [Fact]
    public async Task Reservation_FullFlow_Should_Confirm_Reservation_And_Sell_Seats()
    {
        // Arrange
        using var db = CreateDbContext();
        var screening = await db.Screenings
            .Include(s => s.Seats)
            .SingleAsync(s => s.Id == DataSeeder.Screening1Id);
        var seatId = screening.Seats.First().SeatId;

        // Step 1: Create reservation
        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });
        response.EnsureSuccessStatusCode();

        var reservationId = (await response.Content
            .ReadFromJsonAsync<CreateReservationResponse>())!.ReservationId;

        // Step 2: Run outbox → ReservationCreated → creates Payment
        await RunOutboxProcessorOnce();

        using var dbAfterPayment = CreateDbContext();
        var payment = await dbAfterPayment.Payments
            .SingleAsync(p => p.ReservationId == reservationId);
        Assert.Equal(PaymentStatus.Pending, payment.Status);

        // Step 3: Simulate payment webhook
        var webhookResponse = await Client.PostAsJsonAsync("/api/payments/webhook", new
        {
            paymentId = payment.Id,
            status = "completed"
        });
        webhookResponse.EnsureSuccessStatusCode();

        // Step 4: Run outbox until seats are sold (end of chain)
        await WaitForAsync(async () =>
        {
            using var db = CreateDbContext();
            var seat = await db.Screenings
                .Include(s => s.Seats)
                .Where(s => s.Id == DataSeeder.Screening1Id)
                .SelectMany(s => s.Seats)
                .SingleOrDefaultAsync(s => s.SeatId == seatId);
            return seat?.Status == ScreeningSeatStatus.Sold;
        });

        // Step 5: Verify final state
        using var verifyDb = CreateDbContext();

        var reservation = await verifyDb.Reservations
            .SingleAsync(r => r.Id == reservationId);

        var updatedSeat = await verifyDb.Screenings
            .Include(s => s.Seats)
            .Where(s => s.Id == DataSeeder.Screening1Id)
            .SelectMany(s => s.Seats)
            .SingleAsync(s => s.SeatId == seatId);

        var completedPayment = await verifyDb.Payments
            .SingleAsync(p => p.ReservationId == reservationId);

        var deadLettered = await verifyDb.OutboxMessages
            .AnyAsync(m => m.DeadLetteredAt != null);

        Assert.Equal(ReservationStatus.Confirmed, reservation.Status);
        Assert.Equal(ScreeningSeatStatus.Sold, updatedSeat.Status);
        Assert.Equal(PaymentStatus.Completed, completedPayment.Status);
        Assert.False(deadLettered, "No outbox messages should be dead-lettered");
    }

    [Fact]
    public async Task ExpiredReservation_ShouldRelease_Seats()
    {
        // Arrange
        using var db = CreateDbContext();
        var screening = await db.Screenings
            .Include(s => s.Seats)
            .SingleAsync(s => s.Id == DataSeeder.Screening1Id);
        var seatId = screening.Seats.First().SeatId;

        // create reservation
        var response = await Client.PostAsJsonAsync("/api/Reservations", new
        {
            screeningId = screening.Id,
            seatIds = new[] { seatId }
        });
        response.EnsureSuccessStatusCode();

        // manually expire it in DB
        using var expireDb = CreateDbContext();
        var reservation = await expireDb.Reservations.SingleAsync();
        expireDb.Database.ExecuteSqlRaw(
            "UPDATE \"Reservations\" SET \"ExpiredAt\" = {0} WHERE \"Id\" = {1}",
            DateTime.UtcNow.AddMinutes(-1),
            reservation.Id);

        // Act
        await RunExpirationWorkerOnce();

        // Assert
        using var verifyDb = CreateDbContext();
        var updatedReservation = await verifyDb.Reservations.SingleAsync();
        var updatedSeat = await verifyDb.ScreeningSeats
            .SingleAsync(s => s.SeatId == seatId);

        Assert.Equal(ReservationStatus.Expired, updatedReservation.Status);
        Assert.Equal(ScreeningSeatStatus.Available, updatedSeat.Status);
    }
}
