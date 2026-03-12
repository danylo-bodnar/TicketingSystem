namespace Ticketing.Domain.Common.Exceptions
{
    public class ConcurrencyException : DomainException
    {
        public ConcurrencyException(string message) : base(message) { }
    }
}