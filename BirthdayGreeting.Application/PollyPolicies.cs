using Microsoft.Extensions.Logging;
using Polly;

namespace BirthdayGreeting.Application;

public static class PollyPolicies
{
    public static IAsyncPolicy CreateRetryPolicy(ILogger logger)
    {
        return Policy
            .Handle<Exception>() // Handles exceptions
            .WaitAndRetryAsync(3, retryAttempt =>
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                (exception, timeSpan, retryCount, context) =>
                {
                    logger.LogWarning(
                        "Retry attempt {RetryCount} after {DelaySeconds}s due to error: {ErrorMessage}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        exception.Message
                    );
                });
    }
}