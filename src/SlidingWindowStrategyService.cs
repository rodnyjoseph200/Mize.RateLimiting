using RateLimiting.Extensions;
using RateLimiting.Models;

namespace RateLimiting;

public class SlidingWindowStrategyService : IRateLimitStategyService
{
    private readonly IReadOnlyCollection<RateLimitTracker> _trackers;
    private readonly SemaphoreSlim _mutex = new(1);

    public SlidingWindowStrategyService(RateLimit[] rateLimits)
    {
        ArgumentNullException.ThrowIfNull(rateLimits);
        _trackers = rateLimits.ToRateLimitTrackers();
    }

    public async Task<TimeSpan?> Execute()
    {
        var currentDateTime = DateTimeOffset.UtcNow;
        await _mutex.WaitAsync();

        try
        {
            foreach (var tracker in _trackers)
            {
                tracker.DequeueExpiredEntries(currentDateTime);

                var wait = tracker.GetWaitTime(currentDateTime);
                if (wait.HasValue)
                    return wait;
            }

            foreach (var tracker in _trackers)
                tracker.Enqueue(currentDateTime);

            return null;
        }
        finally
        {
            _ = _mutex.Release();
        }
    }
}