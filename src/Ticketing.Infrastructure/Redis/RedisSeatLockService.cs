using StackExchange.Redis;
using Ticketing.Application.Common.Interfaces;

public class RedisSeatLockService : ISeatLockService
{
    private readonly IConnectionMultiplexer _redis;

    public RedisSeatLockService(IConnectionMultiplexer redis)
    {
        _redis = redis;
    }

    public async Task<string?> TryLockSeatAsync(Guid screeningId, Guid seatId)
    {
        var db = _redis.GetDatabase();
        var key = $"seat:{screeningId}:{seatId}";
        var value = Guid.NewGuid().ToString();

        var acquired = await db.StringSetAsync(
            key,
            value,
            TimeSpan.FromSeconds(10),
            When.NotExists);

        return acquired ? value : null;
    }

    public async Task ReleaseSeatAsync(Guid screeningId, Guid seatId, string lockValue)
    {
        var db = _redis.GetDatabase();
        var key = $"seat:{screeningId}:{seatId}";

        const string script = @"
            if redis.call('GET', KEYS[1]) == ARGV[1] then
                return redis.call('DEL', KEYS[1])
            else
                return 0
            end";

        await db.ScriptEvaluateAsync(
            script,
            new RedisKey[] { key },
            new RedisValue[] { lockValue });
    }
}
