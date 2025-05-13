using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RateLimiting.Models;

namespace RateLimiting.UnitTests.Models;

[TestClass]
public class RateLimitTests
{
    [TestMethod]
    public void Create_WithValidParameters_ShouldSetProperties()
    {
        var expectedCount = 5;
        var expectedWindow = TimeSpan.FromMinutes(1);

        var rateLimit = RateLimit.Create(expectedCount, expectedWindow);

        _ = rateLimit.Count.Should().Be(expectedCount);
        _ = rateLimit.Window.Should().Be(expectedWindow);
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Create_WithNonPositiveCount_ShouldThrowArgumentOutOfRangeException(int invalidCount)
    {
        var window = TimeSpan.FromSeconds(1);

        Action act = () => RateLimit.Create(invalidCount, window);

        _ = act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("Count");
    }

    [DataTestMethod]
    [DataRow(0)]
    [DataRow(-1)]
    public void Create_WithNonPositiveWindow_ShouldThrowArgumentOutOfRangeException(int seconds)
    {
        var count = 1;
        var invalidWindow = TimeSpan.FromSeconds(seconds);

        Action act = () => RateLimit.Create(count, invalidWindow);

        _ = act.Should().Throw<ArgumentOutOfRangeException>()
            .WithParameterName("Window");
    }
}