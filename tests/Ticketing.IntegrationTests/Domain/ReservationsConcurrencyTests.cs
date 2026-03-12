using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Infrastructure.Contexts;

public class ReservationsConcurrencyTests : IClassFixture<TestDatabaseFixture>
{
    private readonly HttpClient _client;
    private readonly TicketingDbContext _dbContext;

    public ReservationsConcurrencyTests(TestDatabaseFixture fixture)
    {
        _client = fixture.Factory.CreateClient();
        _dbContext = fixture.DbContext;
    }

    [Fact]
    public async Task OnlyOneReservationShouldSucceed_ForSameSeat()
    {
        var screening = await _dbContext.Screenings
            .Include(s => s.Seats)
            .FirstAsync();

        var seatId = screening.Seats.First().SeatId;

        var tasks = Enumerable.Range(0, 50).Select(_ => Task.Run(async () =>
        {
            var response = await _client.PostAsJsonAsync("/api/Reservations", new
            {
                screeningId = screening.Id,
                seatIds = new[] { seatId }
            });

            return response;
        }));

        var results = await Task.WhenAll(tasks);

        var successCount = results.Count(r => r.IsSuccessStatusCode);
        var failureCount = results.Count(r => !r.IsSuccessStatusCode);

        Assert.Equal(1, successCount);
        Assert.Equal(49, failureCount);
    }
}