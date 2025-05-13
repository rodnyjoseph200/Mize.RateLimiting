using System.Collections.ObjectModel;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiting.Extensions;
using RateLimiting.Models;

namespace RateLimiting.UnitTests.Extensions;

[TestClass]
public class RateLimitsExtenstionsTests
{
    [TestMethod]
    public void ToRateLimitTrackers_ShouldThrowArgumentException_WhenRateLimitsIsNull()
    {
        ICollection<RateLimit>? rateLimits = null;
        Action act = () => rateLimits!.ToRateLimitTrackers();

        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("At least one RateLimit required*")
            .And.ParamName.Should().Be("rateLimits");
    }

    [TestMethod]
    public void ToRateLimitTrackers_ShouldThrowArgumentException_WhenRateLimitsIsEmpty()
    {
        var rateLimits = new List<RateLimit>();
        Action act = () => rateLimits.ToRateLimitTrackers();

        _ = act.Should().Throw<ArgumentException>()
            .WithMessage("At least one RateLimit required*")
            .And.ParamName.Should().Be("rateLimits");
    }

    [TestMethod]
    public void ToRateLimitTrackers_ShouldReturnTrackers_ForValidRateLimits()
    {
        var rateLimit1 = RateLimit.Create(1, TimeSpan.FromSeconds(1));
        var rateLimit2 = RateLimit.Create(2, TimeSpan.FromSeconds(2));
        var rateLimits = new List<RateLimit> { rateLimit1, rateLimit2 };

        var trackers = rateLimits.ToRateLimitTrackers();

        _ = trackers.Should().BeOfType<ReadOnlyCollection<RateLimitTracker>>();
        _ = trackers.Should().HaveCount(rateLimits.Count);
        _ = trackers[0].Limit.Should().Be(rateLimit1);
        _ = trackers[1].Limit.Should().Be(rateLimit2);
    }
}