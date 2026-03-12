using Ticketing.Contracts.DTOs;
using Ticketing.Domain.Screenings;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IScreeningRepository
    {
        Task<Screening?> GetByIdAsync(Guid screeningId);
        Task AddAsync(Screening screening);
        Task<List<Screening>> GetAllAsync();
        Task<List<SeatDto>> GetAvailableSeatsAsync(Guid screeningId);
    }
}