using Ticketing.Domain.Halls;

namespace Ticketing.Application.Common.Interfaces
{
    public interface IHallRepository
    {
        Task<Hall?> GetByIdAsync(Guid hallId);
        Task AddAsync(Hall hall);
    }
}