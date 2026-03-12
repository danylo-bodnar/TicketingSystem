using StackExchange.Redis;

public class RedisSeatLockService : ISeatLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisSeatLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<bool> TryLockSeatAsync(Guid screeningId, Guid seatId)
    {
        var db = _redis.GetDatabase();
        var key = $"seat:{screeningId}:{seatId}";

        return await db.StringSetAsync(
            key,
            "locked",
            TimeSpan.FromMinutes(5),
            When.NotExists);
    }

    public async Task ReleaseSeatAsync(Guid screeningId, Guid seatId)
    {
        var db = _redis.GetDatabase();
        var key = $"seat:{screeningId}:{seatId}";

        await db.KeyDeleteAsync(key);
    }
}