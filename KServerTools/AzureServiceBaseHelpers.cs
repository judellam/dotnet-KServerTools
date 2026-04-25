namespace KServerTools.Common;

using System.Diagnostics;

/// <summary>
/// Static helpers for services that don't inherit AzureServiceBase but need logged operations.
/// </summary>
internal static class AzureServiceBaseHelpers {
    /// <summary>
    /// Executes an async void operation with Stopwatch-based latency logging.
    /// </summary>
    /// <param name="logger">The logger for structured JSON output.</param>
    /// <param name="operationName">A descriptive name for the operation, used in log messages.</param>
    /// <param name="operation">The async action to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    internal static async Task LoggedOperationAsync(IJsonLogger logger, string operationName, Func<Task> operation, CancellationToken cancellationToken = default) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            await operation().ConfigureAwait(false);
            stopwatch.Stop();
            logger.Info(operationName, stopwatch.ElapsedMilliseconds);
        } catch (OperationCanceledException ex) {
            stopwatch.Stop();
            CancellationSource source = AzureServiceBase<object>.GetCancellationSource(ex, cancellationToken);
            logger.Warn($"Cancelled ({source}): {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        } catch (Exception ex) {
            stopwatch.Stop();
            logger.Error($"Failed: {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Executes an async operation with Stopwatch-based latency logging and returns the result.
    /// </summary>
    /// <typeparam name="T">The return type of the async operation.</typeparam>
    /// <param name="logger">The logger for structured JSON output.</param>
    /// <param name="operationName">A descriptive name for the operation, used in log messages.</param>
    /// <param name="operation">The async function to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The result of the async operation.</returns>
    internal static async Task<T> LoggedOperationAsync<T>(IJsonLogger logger, string operationName, Func<Task<T>> operation, CancellationToken cancellationToken = default) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            T result = await operation().ConfigureAwait(false);
            stopwatch.Stop();
            logger.Info(operationName, stopwatch.ElapsedMilliseconds);
            return result;
        } catch (OperationCanceledException ex) {
            stopwatch.Stop();
            CancellationSource source = AzureServiceBase<object>.GetCancellationSource(ex, cancellationToken);
            logger.Warn($"Cancelled ({source}): {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        } catch (Exception ex) {
            stopwatch.Stop();
            logger.Error($"Failed: {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
