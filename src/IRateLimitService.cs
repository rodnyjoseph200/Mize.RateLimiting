namespace RateLimiting;

public interface IRateLimitService<T>
{
    Task Perform(T arg);
}