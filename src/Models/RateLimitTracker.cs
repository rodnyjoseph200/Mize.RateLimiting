namespace RateLimiting.Models;

public record RateLimitTracker
{
    public RateLimit Limit { get; }

    private readonly Queue<DateTimeOffset> _queue;

    private RateLimitTracker(RateLimit limit, Queue<DateTimeOffset> queue)
    {
        ArgumentNullException.ThrowIfNull(limit);
        ArgumentNullException.ThrowIfNull(queue);

        Limit = limit;
        _queue = queue;
    }

    public static RateLimitTracker Create(RateLimit limit) => new(limit, new());

    public void DequeueExpiredEntries(DateTimeOffset currentDateTime)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(currentDateTime, default);

        while (_queue.TryPeek(out var dateTimeFromQueue) && dateTimeFromQueue <= (currentDateTime - Limit.Window))
            _ = _queue.TryDequeue(out _);
    }

    public TimeSpan? GetWaitTime(DateTimeOffset currentDateTime)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(currentDateTime, default);

        if (_queue.Count >= Limit.Count)
        {
            var oldest = _queue.Peek();
            var wait = oldest + Limit.Window - currentDateTime;
            if (wait > TimeSpan.Zero)
                return wait;
        }

        return null;
    }

    public void Enqueue(DateTimeOffset dateTime)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(dateTime, default);

        if (_queue.Count >= Limit.Count)
            throw new InvalidOperationException("Queue is full");

        _queue.Enqueue(dateTime);
    }
}