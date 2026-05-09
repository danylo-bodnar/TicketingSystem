using Microsoft.Extensions.DependencyInjection;
using Ticketing.Infrastructure.Contexts;
using Ticketing.Application.Outbox;
using Ticketing.Application.Reservations.Services;


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

    protected T GetService<T>() where T : notnull
    {
        var scope = ScopeFactory.CreateScope();
        return scope.ServiceProvider.GetRequiredService<T>();
    }

    protected async Task WaitForAsync(Func<Task<bool>> condition, int timeoutMs = 5000)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            if (await condition()) return;
            await RunOutboxProcessorOnce();
            await Task.Delay(100);
        }
        throw new TimeoutException("Condition not met within timeout — chain may be stuck");
    }

    protected async Task RunExpirationWorkerOnce()
    {
        using var scope = ScopeFactory.CreateScope();
        var service = scope.ServiceProvider
            .GetRequiredService<ReservationExpirationService>();
        await service.ProcessOnce(CancellationToken.None);
    }
}
