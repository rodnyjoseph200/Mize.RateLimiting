using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiting.Models;

namespace RateLimiting.UnitTests.Models;

[TestClass]
public class RateLimitTrackerTests
{
    [TestMethod]
    public void Enqueue_WhenUnderLimit_DoesNotThrow()
    {
        var limit = RateLimit.Create(2, TimeSpan.FromSeconds(10));
        var tracker = RateLimitTracker.Create(limit);
        var now = DateTimeOffset.UtcNow;

        _ = tracker
        .Invoking(t =>
        {
            t.Enqueue(now);
            t.Enqueue(now.AddSeconds(1));
        })
        .Should().NotThrow();
    }

    [TestMethod]
    public void Enqueue_WhenAtLimit_ThrowsInvalidOperationException()
    {
        var limit = RateLimit.Create(1, TimeSpan.FromSeconds(10));
        var tracker = RateLimitTracker.Create(limit);
        var now = DateTimeOffset.UtcNow;

        tracker.Enqueue(now);
        Action act = () => tracker.Enqueue(now.AddSeconds(1));

        _ = act.Should().Throw<InvalidOperationException>().WithMessage("Queue is full");
    }

    [TestMethod]
    public void GetWaitTime_WhenUnderLimit_ReturnsNull()
    {
        var limit = RateLimit.Create(2, TimeSpan.FromSeconds(10));
        var tracker = RateLimitTracker.Create(limit);
        var now = DateTimeOffset.UtcNow;

        tracker.Enqueue(now);
        _ = tracker.GetWaitTime(now).Should().BeNull();
    }

    [TestMethod]
    public void GetWaitTime_WhenAtLimitAndNotExpired_ReturnsPositiveWaitTime()
    {
        var limit = RateLimit.Create(2, TimeSpan.FromSeconds(10));
        var tracker = RateLimitTracker.Create(limit);
        var now = DateTimeOffset.UtcNow;

        tracker.Enqueue(now);
        tracker.Enqueue(now.AddSeconds(1));
        var wait = tracker.GetWaitTime(now.AddSeconds(2));

        _ = wait.Should().BePositive().And.BeCloseTo(TimeSpan.FromSeconds(9), TimeSpan.FromMilliseconds(10));
    }

    [TestMethod]
    public void GetWaitTime_WhenAtLimitAndExpired_ReturnsNull()
    {
        var limit = RateLimit.Create(1, TimeSpan.FromSeconds(1));
        var tracker = RateLimitTracker.Create(limit);
        var now = DateTimeOffset.UtcNow;

        tracker.Enqueue(now);
        var wait = tracker.GetWaitTime(now.AddSeconds(2));

        _ = wait.Should().BeNull();
    }

    [TestMethod]
    public void DequeueExpiredEntries_RemovesExpiredEntries()
    {
        var limit = RateLimit.Create(3, TimeSpan.FromSeconds(1));
        var tracker = RateLimitTracker.Create(limit);
        var now = DateTimeOffset.UtcNow;

        tracker.Enqueue(now);
        tracker.Enqueue(now.AddSeconds(0.5));
        tracker.Enqueue(now.AddSeconds(1.5));
        tracker.DequeueExpiredEntries(now.AddSeconds(2));
        _ = tracker.GetWaitTime(now.AddSeconds(2)).Should().BeNull();

        _ = tracker
        .Invoking(t => t.Enqueue(now.AddSeconds(2)))
        .Should().NotThrow();
    }
}