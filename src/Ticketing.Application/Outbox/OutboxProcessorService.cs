using Ticketing.Application.Common.Interfaces;

namespace Ticketing.Application.Outbox
{
    public class OutboxProcessorService
    {
        private readonly IOutboxRepository _outbox;
        private readonly IEventDispatcher _dispatcher;
        private readonly IUnitOfWork _unitOfWork;

        public OutboxProcessorService(
            IOutboxRepository outbox,
            IEventDispatcher dispatcher,
            IUnitOfWork unitOfWork)
        {
            _outbox = outbox;
            _dispatcher = dispatcher;
            _unitOfWork = unitOfWork;
        }

        public async Task ProcessOnce(CancellationToken ct)
        {
            var messages = await _outbox.GetPendingAsync(20, ct);

            foreach (var msg in messages)
            {
                try
                {
                    await _dispatcher.DispatchAsync(msg.Type, msg.Payload, ct);

                    msg.ProcessedAt = DateTime.UtcNow;
                }
                catch
                {
                    msg.RetryCount++;
                    msg.NextRetryAt = DateTime.UtcNow.AddSeconds(5 * msg.RetryCount);
                }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
