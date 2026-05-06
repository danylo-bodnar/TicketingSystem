using Microsoft.Extensions.Logging;
using Serilog.Context;
using Ticketing.Application.Common.Interfaces;

namespace Ticketing.Application.Outbox
{
    public class OutboxProcessorService
    {
        private readonly IOutboxRepository _outbox;
        private readonly IEventDispatcher _dispatcher;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OutboxProcessorService> _logger;

        public OutboxProcessorService(
            IOutboxRepository outbox,
            IEventDispatcher dispatcher,
            IUnitOfWork unitOfWork,
            ILogger<OutboxProcessorService> logger
            )
        {
            _outbox = outbox;
            _dispatcher = dispatcher;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task ProcessOnce(CancellationToken ct)
        {
            var messages = await _outbox.GetPendingAsync(20, ct);

            foreach (var msg in messages)
            {
                using (LogContext.PushProperty("CorrelationId", msg.CorrelationId))
                using (LogContext.PushProperty("MessageType", msg.Type))
                using (LogContext.PushProperty("MessageId", msg.Id))
                    try
                    {
                        await _dispatcher.DispatchAsync(msg.Type, msg.Payload, ct);

                        msg.ProcessedAt = DateTime.UtcNow;

                        _logger.LogInformation("Outbox message {MessageId} processed", msg.Id);
                    }
                    catch (Exception ex)
                    {
                        msg.RetryCount++;
                        msg.LastError = ex.Message;

                        if (msg.RetryCount >= 5)
                        {
                            msg.DeadLetteredAt = DateTime.UtcNow;

                            _logger.LogError(ex, "Outbox message {MessageId} dead-lettered after 5 retries", msg.Id);
                        }
                        else
                        {
                            msg.NextRetryAt = DateTime.UtcNow.AddSeconds(5 * msg.RetryCount);

                            _logger.LogWarning(ex, "Outbox message {MessageId} failed, retry {RetryCount} scheduled", msg.Id, msg.RetryCount);
                        }
                    }
            }

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
