using Ticketing.Domain.Payments;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IPaymentRepository
    {
        Task AddAsync(Payment payment);
        Task<bool> ExistsByReservationId(Guid reservationId);
    }
}
