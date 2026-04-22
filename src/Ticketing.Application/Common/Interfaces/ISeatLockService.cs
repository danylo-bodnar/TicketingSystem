namespace Ticketing.Application.Common.Interfaces
{
    public interface ISeatLockService
    {
        Task ReleaseSeatAsync(Guid screeningId, Guid seatId, string lockValue);
        Task<string?> TryLockSeatAsync(Guid screeningId, Guid seatId);
    }
}
