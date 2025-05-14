using System;
using RateLimiting.Models;

namespace RateLimiting.Extensions;

public static class RateLimiterTrackersExtensions
{
    public static TimeSpan[] GetWaitTimes(this IReadOnlyCollection<RateLimitTracker> trackers, DateTimeOffset currentDateTime)
    {
        ArgumentNullException.ThrowIfNull(trackers);
        ArgumentOutOfRangeException.ThrowIfEqual(currentDateTime, default);

        var waits = trackers
            .Select(t => t.GetWaitTime(currentDateTime))
            .Where(w => w.HasValue)
            .Select(w => w!.Value)
            .ToArray();

        return waits ?? [];
    }

    public static TimeSpan? GetMaxWaitTime(this IReadOnlyCollection<RateLimitTracker> trackers, DateTimeOffset currentDateTime)
    {
        var waits = GetWaitTimes(trackers, currentDateTime);
        return waits.Length is 0 ? null : waits.Max();
    }

}
