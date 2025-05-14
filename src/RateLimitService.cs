namespace RateLimiting;

public class RateLimitService<TArg> : IRateLimitService<TArg>
{
    // IRateLimitStategyService enables you to use any implementation (sliding window, absolute window)
    private readonly IRateLimitStategyService _rateLimitStategyService;
    private readonly Func<TArg, Task> _action;
    private readonly SemaphoreSlim _mutex = new(1);

    public RateLimitService(IRateLimitStategyService rateLimitStrategyService, Func<TArg, Task> action)
    {
        _rateLimitStategyService = rateLimitStrategyService ?? throw new ArgumentNullException(nameof(rateLimitStrategyService));
        _action = action ?? throw new ArgumentNullException(nameof(action));
    }

    public async Task Perform(TArg arg)
    {
        await _mutex.WaitAsync();
        try
        {
            while (true)
            {
                var wait = _rateLimitStategyService.Execute();
                if (!wait.HasValue)
                    break;

                // can wait, log, and/or throw an exception
                await Task.Delay(wait.Value);
                continue;
            }
        }
        finally
        {
            _mutex.Release();
        }

        await _action(arg);
    }
}