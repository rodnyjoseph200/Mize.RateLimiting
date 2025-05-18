using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiting.Extensions;
using RateLimiting.Models;

namespace RateLimiting.UnitTests.Extensions;

[TestClass]
public class RateLimitTrackersExtensionsTests
{
    [TestMethod]
    public void GetMaxWaitTime_NoWaits_ReturnsNull()
    {
        var now = DateTimeOffset.UtcNow;
        var track = RateLimitTracker.Create(RateLimit.Create(2, TimeSpan.FromSeconds(10)));

        track.Enqueue(now);
        var result = new[] { track }.GetMaxWaitTime(now);

        _ = result.Should().BeNull();
    }

    [TestMethod]
    public void GetMaxWaitTime_WithWaits_ReturnsMaxWait()
    {
        var now = DateTimeOffset.UtcNow;
        var t1 = RateLimitTracker.Create(RateLimit.Create(1, TimeSpan.FromSeconds(5)));
        t1.Enqueue(now);
        var t2 = RateLimitTracker.Create(RateLimit.Create(1, TimeSpan.FromSeconds(10)));
        t2.Enqueue(now);

        var result = new[] { t1, t2 }.GetMaxWaitTime(now);

        _ = result.Should().BeCloseTo(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(10));
    }

    [TestMethod]
    public void GetWaitTimes_NullTrackers_ThrowsArgumentNullException()
    {
        Action act = () => ((IReadOnlyCollection<RateLimitTracker>)null!).GetWaitTimes(DateTimeOffset.UtcNow);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("trackers");
    }

    [TestMethod]
    public void GetWaitTimes_DefaultDate_ThrowsArgumentOutOfRangeException()
    {
        var trackers = new List<RateLimitTracker> { RateLimitTracker.Create(RateLimit.Create(1, TimeSpan.FromSeconds(1))) };
        Action act = () => trackers.GetWaitTimes(default);
        _ = act.Should().Throw<ArgumentOutOfRangeException>().WithParameterName("currentDateTime");
    }

    [TestMethod]
    public void GetWaitTimes_NoWaits_ReturnsEmptyArray()
    {
        var now = DateTimeOffset.UtcNow;
        var track = RateLimitTracker.Create(RateLimit.Create(2, TimeSpan.FromSeconds(10)));

        track.Enqueue(now);
        var waits = new[] { track }.GetWaitTimes(now);

        _ = waits.Should().BeEmpty();
    }

    [TestMethod]
    public void GetWaitTimes_WithWaits_ReturnsCorrectWaits()
    {
        var now = DateTimeOffset.UtcNow;
        var t1 = RateLimitTracker.Create(RateLimit.Create(1, TimeSpan.FromSeconds(5)));
        t1.Enqueue(now);
        var t2 = RateLimitTracker.Create(RateLimit.Create(1, TimeSpan.FromSeconds(10)));
        t2.Enqueue(now);

        var waits = new[] { t1, t2 }.GetWaitTimes(now);

        _ = waits.Should().HaveCount(2);
        _ = waits[0].Should().BeCloseTo(TimeSpan.FromSeconds(5), TimeSpan.FromMilliseconds(10));
        _ = waits[1].Should().BeCloseTo(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(10));
    }
}