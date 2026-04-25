namespace KServerTools.Common;

using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Common base class for Azure service wrappers.
/// Provides shared memory caching with namespaced keys and logged async operations.
/// </summary>
internal abstract class AzureServiceBase<TConfig>(TConfig config, IMemoryCache memoryCache, string credentialId, IJsonLogger? logger = null) where TConfig : class {
    protected readonly TConfig config = config;
    protected readonly IMemoryCache memoryCache = memoryCache;
    protected readonly IJsonLogger? logger = logger;
    private readonly string credentialId = credentialId;

    protected static readonly MemoryCacheEntryOptions DefaultCacheOptions = new() {
        SlidingExpiration = TimeSpan.FromMinutes(50)
    };

    /// <summary>
    /// Executes an async operation with Stopwatch-based latency logging.
    /// Logs success on completion, logs error and rethrows on failure.
    /// Cancellations are logged as warnings with source attribution (caller vs server).
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    protected async Task<T> LoggedOperationAsync<T>(string operationName, Func<Task<T>> operation, CancellationToken cancellationToken = default) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            T result = await operation().ConfigureAwait(false);
            stopwatch.Stop();
            this.logger?.Info(operationName, stopwatch.ElapsedMilliseconds);
            return result;
        } catch (OperationCanceledException ex) {
            stopwatch.Stop();
            CancellationSource source = GetCancellationSource(ex, cancellationToken);
            this.logger?.Warn($"Cancelled ({source}): {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        } catch (Exception ex) {
            stopwatch.Stop();
            this.logger?.Error($"Failed: {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Executes an async void operation with Stopwatch-based latency logging.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    protected async Task LoggedOperationAsync(string operationName, Func<Task> operation, CancellationToken cancellationToken = default) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            await operation().ConfigureAwait(false);
            stopwatch.Stop();
            this.logger?.Info(operationName, stopwatch.ElapsedMilliseconds);
        } catch (OperationCanceledException ex) {
            stopwatch.Stop();
            CancellationSource source = GetCancellationSource(ex, cancellationToken);
            this.logger?.Warn($"Cancelled ({source}): {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        } catch (Exception ex) {
            stopwatch.Stop();
            this.logger?.Error($"Failed: {operationName}", ex, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    /// <summary>
    /// Gets or creates a cached value. Cache keys are automatically prefixed with the credential
    /// identity to isolate cached clients/data across different credential contexts in multi-tenant scenarios.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    protected async Task<T> GetOrCreateCachedAsync<T>(string cacheKey, Func<Task<T>> factory, MemoryCacheEntryOptions? options = null) where T : notnull {
        string scopedKey = $"{this.credentialId}:{cacheKey}";
        if (this.memoryCache.TryGetValue(scopedKey, out T? cached) && cached is not null) {
            return cached;
        }

        T value = await factory().ConfigureAwait(false);
        this.memoryCache.Set(scopedKey, value, options ?? DefaultCacheOptions);
        return value;
    }

    protected static void VerifyArgs(params string[] args) {
        foreach (string arg in args) {
            if (string.IsNullOrWhiteSpace(arg)) {
                throw new ArgumentException("Argument cannot be null or empty.");
            }
        }
    }

    /// <summary>
    /// Determines whether a cancellation was initiated by the caller (e.g., client disconnect,
    /// HttpContext.RequestAborted) or by the server (e.g., internal timeout, shutdown).
    /// </summary>
    /// <returns></returns>
    internal static CancellationSource GetCancellationSource(OperationCanceledException ex, CancellationToken callerToken) {
        if (callerToken.IsCancellationRequested) {
            return CancellationSource.Caller;
        }

        if (ex.CancellationToken != CancellationToken.None && ex.CancellationToken.IsCancellationRequested) {
            return CancellationSource.Server;
        }

        return CancellationSource.Unknown;
    }
}

/// <summary>
/// Static helpers for services that don't inherit AzureServiceBase but need logged operations.
/// </summary>
internal static class AzureServiceBaseHelpers {
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
