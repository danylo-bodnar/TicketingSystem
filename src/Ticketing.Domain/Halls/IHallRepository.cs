namespace Ticketing.Domain.Halls
{
    public interface IHallRepository
    {
        Task<Hall?> GetByIdAsync(Guid hallId);
        Task AddAsync(Hall hall);
    }
}
