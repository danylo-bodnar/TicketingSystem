using Ticketing.Infrastructure.Contexts;
using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Interfaces;

namespace Ticketing.Worker.Jobs
{
    public class OutboxProcessor : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public OutboxProcessor(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var db = scope.ServiceProvider.GetRequiredService<TicketingDbContext>();
                var dispatcher = scope.ServiceProvider.GetRequiredService<IEventDispatcher>();

                var messages = await db.OutboxMessages
                    .Where(x => x.ProcessedAt == null)
                    .Take(20)
                    .ToListAsync(ct);

                foreach (var msg in messages)
                {
                    try
                    {
                        await dispatcher.DispatchAsync(msg.Type, msg.Payload);

                        msg.ProcessedAt = DateTime.UtcNow;
                    }
                    catch (Exception ex)
                    {
                        // IMPORTANT: do not mark as processed
                        Console.WriteLine($"Failed to process {msg.Type}: {ex.Message}");
                    }
                }

                await db.SaveChangesAsync(ct);

                await Task.Delay(1000, ct);
            }
        }
    }
}