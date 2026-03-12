namespace Ticketing.Domain.Common.Exceptions
{
    public class InvalidCurrencyException : DomainException
    {
        public InvalidCurrencyException(string currency) : base($"Invalid currency: {currency}")
        {
        }
    }
}