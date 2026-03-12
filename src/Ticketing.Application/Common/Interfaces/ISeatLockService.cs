public interface ISeatLockService
{
    Task<bool> TryLockSeatAsync(Guid screeningId, Guid seatId);
    Task ReleaseSeatAsync(Guid screeningId, Guid seatId);
}