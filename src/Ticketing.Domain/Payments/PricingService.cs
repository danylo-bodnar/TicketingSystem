using Ticketing.Domain.Common.ValueObjects;

namespace Ticketing.Domain.Payments
{
    public class PricingService
    {
        public Money Calculate(int seatCount, decimal occupancyRatio)
        {
            var basePrice = 100m;
            var multiplier = occupancyRatio switch
            {
                < 0.3m => 1.0m,
                < 0.6m => 1.2m,
                < 0.8m => 1.5m,
                _ => 2.0m
            };

            return new Money(seatCount * basePrice * multiplier, "USD");
        }
    }
}
