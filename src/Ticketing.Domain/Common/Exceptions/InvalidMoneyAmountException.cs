namespace Ticketing.Domain.Common.Exceptions
{
    public class InvalidMoneyAmountException : DomainException
    {
        public InvalidMoneyAmountException(decimal amount) : base($"Money amount cannot be negative: {amount}")
        {
        }
    }
}