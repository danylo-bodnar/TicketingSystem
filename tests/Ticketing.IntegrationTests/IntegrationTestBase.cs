using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Application.Outbox;

[Collection("Integration")]
public abstract class IntegrationTestBase
{
    protected readonly HttpClient Client;
    protected readonly IServiceScopeFactory ScopeFactory;

    protected IntegrationTestBase(TestDatabaseFixture fixture)
    {
        Client = fixture.Factory.CreateClient();
        ScopeFactory = fixture.Factory.Services.GetRequiredService<IServiceScopeFactory>();

        fixture.ResetDatabase();
    }

    protected TicketingDbContext CreateDbContext()
    {
        var scope = ScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
    }

    protected async Task RunOutboxProcessorOnce()
    {
        using var scope = ScopeFactory.CreateScope();

        var processor = scope.ServiceProvider
            .GetRequiredService<OutboxProcessorService>();

        await processor.ProcessOnce(CancellationToken.None);
    }

    protected T GetService<T>()
    {
        var scope = ScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

}
