namespace RateLimiting;

public interface IRateLimitStategyService
{
    Task<TimeSpan?> Execute();
}