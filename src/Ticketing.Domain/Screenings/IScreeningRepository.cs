using Ticketing.Domain.Screenings;
using Ticketing.Domain.Seats;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IScreeningRepository
    {
        Task<Screening?> GetByIdAsync(Guid screeningId, CancellationToken ct = default);
        Task AddAsync(Screening screening, CancellationToken ct = default);
        Task<List<Screening>> GetAllAsync(CancellationToken ct = default);
        Task<List<ScreeningSeat>> GetAvailableSeatsAsync(Guid screeningId, CancellationToken ct = default);
    }
}