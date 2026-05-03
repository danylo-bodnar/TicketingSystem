namespace Ticketing.Domain.Payments
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment, CancellationToken ct = default);
        Task<Payment?> GetById(Guid paymentId, CancellationToken ct = default);
    }
}
