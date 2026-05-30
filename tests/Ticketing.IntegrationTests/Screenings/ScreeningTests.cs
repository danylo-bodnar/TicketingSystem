using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Ticketing.Contracts.DTOs;
using Ticketing.Contracts.Screenings;
using Ticketing.Domain.Screenings;

namespace Ticketing.IntegrationTests.Screenings;

[Collection("Integration")]
public class ScreeningTests : IntegrationTestBase
{
    public ScreeningTests(TestDatabaseFixture fixture) : base(fixture) { }

    [Fact]
    public async Task GetAll_ShouldReturnScreenings()
    {
        var response = await Client.GetAsync("/api/Screenings");
        response.EnsureSuccessStatusCode();

        var screenings = await response.Content.ReadFromJsonAsync<List<ScreeningResponse>>();
        Assert.NotNull(screenings);
        Assert.NotEmpty(screenings);
    }

    [Fact]
    public async Task GetSeats_ShouldReturnSeatsForScreening()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();

        var response = await Client.GetAsync($"/api/Screenings/{screening.Id}/seats");
        response.EnsureSuccessStatusCode();

        var seats = await response.Content.ReadFromJsonAsync<List<SeatDto>>();
        Assert.NotNull(seats);
        Assert.NotEmpty(seats);
        Assert.Equal(screening.Seats.Count, seats.Count);
    }

    [Fact]
    public async Task GetSeats_WhenScreeningNotFound_ShouldReturnBadRequest()
    {
        var response = await Client.GetAsync($"/api/Screenings/{Guid.NewGuid()}/seats");

        Assert.False(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task GetAvailableSeats_ShouldReturnOnlyAvailable()
    {
        using var db = CreateDbContext();
        var screening = await db.Screenings.Include(s => s.Seats).FirstAsync();
        var availableCount = screening.Seats.Count(s => s.Status == ScreeningSeatStatus.Available);

        var response = await Client.GetAsync($"/api/Screenings/{screening.Id}/available-seats");
        response.EnsureSuccessStatusCode();

        var seats = await response.Content.ReadFromJsonAsync<List<SeatDto>>();
        Assert.NotNull(seats);
        Assert.Equal(availableCount, seats.Count);
        Assert.All(seats, s => Assert.Equal(ScreeningSeatStatus.Available, s.Status));
    }
}
