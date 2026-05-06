namespace Ticketing.Application.Common
{
    public class CorrelationContext
    {
        public string CorrelationId { get; set; } = Guid.NewGuid().ToString();
    }
}