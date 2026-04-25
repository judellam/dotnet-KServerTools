namespace KServerTools.Common;

using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Common base class for Azure service wrappers.
/// Provides shared memory caching with namespaced keys and logged async operations.
/// </summary>
/// <typeparam name="TConfig">The configuration type for the Azure service.</typeparam>
/// <param name="config">The service configuration instance.</param>
/// <param name="memoryCache">The shared memory cache.</param>
/// <param name="credentialId">An identity string used to namespace cache keys across credential contexts.</param>
/// <param name="logger">An optional logger for structured JSON output.</param>
internal abstract class AzureServiceBase<TConfig>(TConfig config, IMemoryCache memoryCache, string credentialId, IJsonLogger? logger = null) where TConfig : class {
    protected static readonly MemoryCacheEntryOptions DefaultCacheOptions = new() {
        SlidingExpiration = TimeSpan.FromMinutes(50)
    };

    protected readonly TConfig config = config;
    protected readonly IMemoryCache memoryCache = memoryCache;
    protected readonly IJsonLogger? logger = logger;
    private readonly string credentialId = credentialId;

    /// <summary>
    /// Determines whether a cancellation was initiated by the caller (e.g., client disconnect,
    /// HttpContext.RequestAborted) or by the server (e.g., internal timeout, shutdown).
    /// </summary>
    /// <param name="ex">The cancellation exception that was thrown.</param>
    /// <param name="callerToken">The token passed by the caller to detect caller-initiated cancellation.</param>
    /// <returns>A <see cref="CancellationSource"/> value indicating who initiated the cancellation.</returns>
    internal static CancellationSource GetCancellationSource(OperationCanceledException ex, CancellationToken callerToken) {
        if (callerToken.IsCancellationRequested) {
            return CancellationSource.Caller;
        }

        if (ex.CancellationToken != CancellationToken.None && ex.CancellationToken.IsCancellationRequested) {
            return CancellationSource.Server;
        }

        return CancellationSource.Unknown;
    }

    /// <summary>
    /// Validates that none of the provided string arguments are null or whitespace.
    /// </summary>
    /// <param name="args">The string arguments to validate.</param>
    protected static void VerifyArgs(params string[] args) {
        foreach (string arg in args) {
            if (string.IsNullOrWhiteSpace(arg)) {
                throw new ArgumentException("Argument cannot be null or empty.");
            }
        }
    }

    /// <summary>
    /// Executes an async operation with Stopwatch-based latency logging.
    /// Logs success on completion, logs error and rethrows on failure.
    /// Cancellations are logged as warnings with source attribution (caller vs server).
    /// </summary>
    /// <typeparam name="T">The return type of the async operation.</typeparam>
    /// <param name="operationName">A descriptive name for the operation, used in log messages.</param>
    /// <param name="operation">The async function to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>The result of the async operation.</returns>
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
    /// <param name="operationName">A descriptive name for the operation, used in log messages.</param>
    /// <param name="operation">The async action to execute.</param>
    /// <param name="cancellationToken">A token to observe for cancellation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
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
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="cacheKey">The logical cache key (will be prefixed with the credential identity).</param>
    /// <param name="factory">A factory function invoked on cache miss to produce the value.</param>
    /// <param name="options">Optional cache entry options; defaults to <see cref="DefaultCacheOptions"/>.</param>
    /// <returns>The cached or newly created value.</returns>
    protected async Task<T> GetOrCreateCachedAsync<T>(string cacheKey, Func<Task<T>> factory, MemoryCacheEntryOptions? options = null) where T : notnull {
        string scopedKey = $"{this.credentialId}:{cacheKey}";
        if (this.memoryCache.TryGetValue(scopedKey, out T? cached) && cached is not null) {
            return cached;
        }

        T value = await factory().ConfigureAwait(false);
        this.memoryCache.Set(scopedKey, value, options ?? DefaultCacheOptions);
        return value;
    }
}
