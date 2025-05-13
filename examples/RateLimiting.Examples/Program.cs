using Microsoft.Extensions.DependencyInjection;
using RateLimiting;
using RateLimiting.Models;

var services = new ServiceCollection();

services.AddSingleton<IRateLimitStategyService>(_ =>
    new SlidingWindowStrategyService([
        RateLimit.Create(1, TimeSpan.FromSeconds(5)),
        RateLimit.Create(2, TimeSpan.FromMinutes(1)),
        RateLimit.Create(3, TimeSpan.FromHours(1))
    ]));

services.AddSingleton<IRateLimitService<string>>(sp =>
    new RateLimitService<string>(
        sp.GetRequiredService<IRateLimitStategyService>(),
        CallApi
    )
);

using var provider = services.BuildServiceProvider();

var rateLimitService = provider.GetRequiredService<IRateLimitService<string>>();

var call1Task = rateLimitService.Perform("this is from call 1");
var call2Task = rateLimitService.Perform("this is from call 2");
var call3Task = rateLimitService.Perform("this is from call 3");

await Task.WhenAll(call1Task, call2Task, call3Task);

Console.WriteLine("Rate limiting example completed.");
Console.ReadLine();

static async Task CallApi(string message)
{
    await Task.Delay(1000); // Simulate network delay
    Console.WriteLine($"{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} > API called with message: {message}");
}