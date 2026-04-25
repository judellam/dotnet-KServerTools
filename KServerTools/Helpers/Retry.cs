namespace KServerTools.Common;

/// <summary>
/// Retry helper with exponential backoff and jitter.
/// </summary>
public static class Retry {
    private static readonly Random Jitter = new();

    /// <summary>
    /// Retries an async action with exponential backoff and jitter.
    /// </summary>
    /// <param name="action">The action to retry.</param>
    /// <param name="maxRetries">Maximum number of attempts (default: 3).</param>
    /// <param name="delay">Base delay in milliseconds before applying backoff (default: 1000).</param>
    /// <param name="shouldRetry">Optional predicate to determine if an exception is retryable. Defaults to retrying all exceptions.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task DoAsync(Func<Task> action, int maxRetries = 3, int delay = 1000, Func<Exception, bool>? shouldRetry = null) {
        var exceptions = new List<Exception>();
        for (var i = 0; i < maxRetries; i++) {
            try {
                await action().ConfigureAwait(false);
                return;
            } catch (Exception ex) when (i < maxRetries - 1 && (shouldRetry?.Invoke(ex) ?? true)) {
                exceptions.Add(ex);
                await Task.Delay(ComputeDelay(delay, i)).ConfigureAwait(false);
            } catch (Exception ex) {
                exceptions.Add(ex);
                break;
            }
        }

        throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Retries an async function with exponential backoff and jitter.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    public static async Task<T> DoAsync<T>(Func<Task<T>> action, int maxRetries = 3, int delay = 1000, Func<Exception, bool>? shouldRetry = null) {
        var exceptions = new List<Exception>();
        for (var i = 0; i < maxRetries; i++) {
            try {
                return await action().ConfigureAwait(false);
            } catch (Exception ex) when (i < maxRetries - 1 && (shouldRetry?.Invoke(ex) ?? true)) {
                exceptions.Add(ex);
                await Task.Delay(ComputeDelay(delay, i)).ConfigureAwait(false);
            } catch (Exception ex) {
                exceptions.Add(ex);
                break;
            }
        }

        throw new AggregateException(exceptions);
    }

    /// <summary>
    /// Computes delay with exponential backoff and decorrelated jitter.
    /// Uses the "full jitter" algorithm: delay = random(0, baseDelay * 2^attempt).
    /// </summary>
    /// <returns></returns>
    internal static int ComputeDelay(int baseDelay, int attempt) {
        var maxDelay = baseDelay * (1 << Math.Min(attempt, 8));
        lock (Jitter) {
            return Jitter.Next(maxDelay / 2, maxDelay);
        }
    }
}
