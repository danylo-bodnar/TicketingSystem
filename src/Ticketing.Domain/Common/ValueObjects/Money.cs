using Ticketing.Domain.Common.Exceptions;

namespace Ticketing.Domain.Common.ValueObjects
{
    public sealed class Money
    {
        public decimal Amount { get; }
        public string Currency { get; }

        public Money(decimal amount, string currency)
        {
            if (amount < 0)
            {
                throw new InvalidMoneyAmountException(amount);
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                throw new InvalidCurrencyException(currency);
            }

            Amount = amount;
            Currency = currency;
        }

        public Money Add(Money other)
        {
            if (other.Currency != Currency)
            {
                throw new CurrencyMismatchException(Currency, other.Currency);
            }

            return new Money(Amount + other.Amount, Currency);
        }

        public Money Subtract(Money other)
        {
            if (other.Currency != Currency)
            {
                throw new CurrencyMismatchException(Currency, other.Currency);
            }

            var result = Amount - other.Amount;
            if (result < 0)
            {
                throw new InvalidMoneyAmountException(result);
            }

            return new Money(result, Currency);
        }
    }
}