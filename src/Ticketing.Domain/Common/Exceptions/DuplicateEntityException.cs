namespace Ticketing.Domain.Common.Exceptions
{
    public class DuplicateEntityException : DomainException
    {
        public DuplicateEntityException() : base("Entity already exists.") { }
    }
}
