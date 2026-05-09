using Ticketing.Domain.Reservations;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IReservationRepository
    {
        Task AddAsync(Reservation reservation, CancellationToken ct = default);
        Task<Reservation?> GetByIdAsync(Guid reservationId, CancellationToken ct = default);
        Task<List<Reservation>> GetExpiredAsync(CancellationToken ct = default);
    }
}