namespace Ticketing.Contracts.DTOs
{
    public class PaymentWebhookDto
    {
        public Guid PaymentId { get; set; }
        public string Status { get; set; } = null!; // "completed" | "failed"
    }
}