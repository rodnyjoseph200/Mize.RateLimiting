namespace RateLimiting.Models;

public record RateLimit
{
    public int Count { get; }
    public TimeSpan Window { get; }

    private RateLimit(int count, TimeSpan window)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(count);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(window, TimeSpan.Zero);

        Count = count;
        Window = window;
    }

    public static RateLimit Create(int count, TimeSpan window)
    {
        return new RateLimit(count, window);
    }
}