using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Infrastructure.Seed;
using Ticketing.IntegrationTests.Fixtures;

public class TestDatabaseFixture : IDisposable
{
    public CustomWebApplicationFactory Factory { get; }

    public IServiceScopeFactory ScopeFactory =>
        Factory.Services.GetRequiredService<IServiceScopeFactory>();

    public TestDatabaseFixture()
    {
        Factory = new CustomWebApplicationFactory();

        using var scope = Factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();


        db.Database.EnsureDeleted();
        db.Database.Migrate();
    }

    public void ResetDatabase()
    {
        using var scope = ScopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();

        var tableNames = db.Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(t => t != null)
            .Distinct()
            .Select(t => $"\"{t}\"")
            .ToList();

        db.Database.ExecuteSqlRaw(
            $"TRUNCATE TABLE {string.Join(", ", tableNames)} CASCADE;"
        );

        DataSeeder.Seed(db);
    }

    public void Dispose() => Factory.Dispose();
}
