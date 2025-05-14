namespace RateLimiting;

public interface IRateLimitStategyService
{
    TimeSpan? Execute();
}