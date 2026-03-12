using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Infrastructure.Seed;
using Ticketing.IntegrationTests.Fixtures;

public class TestDatabaseFixture : IDisposable
{
    public CustomWebApplicationFactory Factory { get; }
    public TicketingDbContext DbContext { get; }

    public TestDatabaseFixture()
    {
        Factory = new CustomWebApplicationFactory();
        var scope = Factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        // Clean DB
        DbContext.Database.ExecuteSqlRaw(
            "TRUNCATE TABLE \"Reservations\", \"ScreeningSeats\", \"Screenings\", \"Seats\", \"Halls\" CASCADE;"
        );

        // Seed data using your centralized DataSeeder
        DataSeeder.Seed(DbContext);
    }

    public void Dispose()
    {
        DbContext.Dispose();
        Factory.Dispose();
    }
}