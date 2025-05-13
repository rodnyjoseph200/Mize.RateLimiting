using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace RateLimiting.UnitTests;

[TestClass]
public class RateLimitServiceTests
{
    [TestMethod]
    public void Constructor_NullStrategy_ThrowsArgumentNullException()
    {
        Action act = () => new RateLimitService<int>(null!, _ => Task.CompletedTask);
        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("rateLimitStrategyService");
    }

    [TestMethod]
    public void Constructor_NullAction_ThrowsArgumentNullException()
    {
        var strategyMock = new Mock<IRateLimitStategyService>();
        Action act = () => new RateLimitService<int>(strategyMock.Object, null!);

        _ = act.Should().Throw<ArgumentNullException>().WithParameterName("action");
    }

    [TestMethod]
    public async Task Perform_WhenExecuteReturnsNull_CallsActionOnce()
    {
        var strategyMock = new Mock<IRateLimitStategyService>();

        _ = strategyMock.Setup(s => s.Execute()).ReturnsAsync((TimeSpan?)null);

        var calls = new List<int>();
        Task action(int x)
        {
            calls.Add(x);
            return Task.CompletedTask;
        }

        var service = new RateLimitService<int>(strategyMock.Object, action);
        await service.Perform(42);

        strategyMock.Verify(s => s.Execute(), Times.Once);
        _ = calls.Should().ContainSingle().Which.Should().Be(42);
    }

    [TestMethod]
    public async Task Perform_WhenExecuteReturnsWaits_LoopsAndCallsAction()
    {
        var strategyMock = new Mock<IRateLimitStategyService>();

        _ = strategyMock.SetupSequence(s => s.Execute())
            .ReturnsAsync(TimeSpan.Zero)
            .ReturnsAsync((TimeSpan?)null);

        var calls = new List<string>();
        Task action(string s)
        {
            calls.Add(s);
            return Task.CompletedTask;
        }

        var service = new RateLimitService<string>(strategyMock.Object, action);
        await service.Perform("test");

        strategyMock.Verify(s => s.Execute(), Times.Exactly(2));
        _ = calls.Should().ContainSingle().Which.Should().Be("test");
    }
}