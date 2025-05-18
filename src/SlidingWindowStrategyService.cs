using RateLimiting.Extensions;
using RateLimiting.Models;

namespace RateLimiting;

public class SlidingWindowStrategyService : IRateLimitStategyService
{
    private readonly IReadOnlyCollection<RateLimitTracker> _trackers;

    public SlidingWindowStrategyService(RateLimit[] rateLimits)
    {
        ArgumentNullException.ThrowIfNull(rateLimits);
        _trackers = rateLimits.ToRateLimitTrackers();
    }

    public TimeSpan? Execute()
    {
        var currentDateTime = DateTimeOffset.UtcNow;

        foreach (var tracker in _trackers)
            tracker.DequeueExpiredEntries(currentDateTime);

        var maxWait = _trackers.GetMaxWaitTime(currentDateTime);

        if (maxWait.HasValue)
            return maxWait;

        foreach (var tracker in _trackers)
            tracker.Enqueue(currentDateTime);

        return null;
    }
}