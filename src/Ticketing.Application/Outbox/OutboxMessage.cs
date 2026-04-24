namespace Ticketing.Application.Outbox
{
    public class OutboxMessage
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;

        public int RetryCount { get; set; }
        public DateTime? NextRetryAt { get; set; }

        public DateTime OccurredAt { get; set; }
        public DateTime? ProcessedAt { get; set; }
    }
}
