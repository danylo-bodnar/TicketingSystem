namespace Ticketing.Domain.Common.Exceptions
{
    public class CurrencyMismatchException : DomainException
    {
        public CurrencyMismatchException(string left, string right) : base($"Currency mismatch: {left} vs {right}")
        {
        }
    }
}