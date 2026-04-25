using Ticketing.Domain.Reservations;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IReservationRepository
    {
        Task AddAsync(Reservation reservation);
        Task<Reservation?> GetByIdAsync(Guid reservationId);
    }
}