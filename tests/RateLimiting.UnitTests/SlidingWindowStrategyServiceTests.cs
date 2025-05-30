using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiting.Models;

namespace RateLimiting.UnitTests;

[TestClass]
public class SlidingWindowStrategyServiceTests
{
    [TestMethod]
    public void Constructor_NullRateLimits_ThrowsArgumentNullException()
    {
        var act = () => new SlidingWindowStrategyService(null!);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("rateLimits");
    }

    [TestMethod]
    public void Execute_WhenUnderLimit_ReturnsNull()
    {
        var limits = new[] { RateLimit.Create(2, TimeSpan.FromSeconds(10)) };
        var service = new SlidingWindowStrategyService(limits);

        var result = service.Execute();

        _ = result.Should().BeNull();
    }

    [TestMethod]
    public void Execute_WhenAtLimit_ReturnsPositiveWaitTime()
    {
        var limits = new[] { RateLimit.Create(1, TimeSpan.FromSeconds(10)) };
        var service = new SlidingWindowStrategyService(limits);

        _ = service.Execute();
        var wait = service.Execute();

        _ = wait.Should().BePositive().And.BeCloseTo(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(50));
    }

    [TestMethod]
    public void Execute_WhenMultipleLimits_FirstWaitIsBasedOnBiggestWindow()
    {
        var small = RateLimit.Create(1, TimeSpan.FromSeconds(5));
        var large = RateLimit.Create(1, TimeSpan.FromSeconds(10));
        var service = new SlidingWindowStrategyService(new[] { small, large });

        _ = service.Execute();
        var wait = service.Execute();

        _ = wait.Should().BePositive().And.BeCloseTo(TimeSpan.FromSeconds(10), TimeSpan.FromMilliseconds(50));
    }
}