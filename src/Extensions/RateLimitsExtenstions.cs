using System.Collections.ObjectModel;
using RateLimiting.Models;

namespace RateLimiting.Extensions;

public static class RateLimitsExtenstions
{
    public static ReadOnlyCollection<RateLimitTracker> ToRateLimitTrackers(this ICollection<RateLimit> rateLimits)
    {
        if (rateLimits is null || rateLimits.Count is 0)
            throw new ArgumentException("At least one RateLimit required", nameof(rateLimits));

        var trackers = rateLimits.Select(RateLimitTracker.Create).ToArray();
        return trackers.AsReadOnly();
    }
}