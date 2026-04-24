using Microsoft.EntityFrameworkCore;
using Ticketing.Application.Common.Interfaces;
using Ticketing.Application.Outbox;
using Ticketing.Infrastructure.Contexts;

namespace Ticketing.Infrastructure.Outbox;


public class OutboxRepository : IOutboxRepository
{
    private readonly TicketingDbContext _db;

    public OutboxRepository(TicketingDbContext db)
    {
        _db = db;
    }

    public async Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct)
    {
        return await _db.OutboxMessages
            .Where(x =>
                x.ProcessedAt == null &&
                (x.NextRetryAt == null || x.NextRetryAt <= DateTime.UtcNow))
            .OrderBy(x => x.OccurredAt)
            .Take(batchSize)
            .ToListAsync(ct);
    }
}
