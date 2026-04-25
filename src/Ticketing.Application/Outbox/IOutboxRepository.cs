using Ticketing.Application.Outbox;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IOutboxRepository
    {
        Task<List<OutboxMessage>> GetPendingAsync(int batchSize, CancellationToken ct);
    }
}
