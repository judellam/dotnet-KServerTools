namespace KServerTools.Common;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// JSON logger implementation that writes log events to Azure Blob Storage with periodic flushing.
/// </summary>
/// <typeparam name="T">The storage log configuration type.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
internal class JsonStorageLogger<T, C> : IJsonLogger where T : AzureStorageServiceLogConfig where C : ITokenCredentialService {
    private const int MaxLogQueueSize = 1000;
    private readonly AzureStorageServiceInternal<T, C> azureStorageService;
    private readonly IHttpContextAccessor accessor;
    private readonly ILogger<JsonLogger> logger;
    private readonly IRequestContextAccessor requestContextAccessor;
    private readonly ConcurrentQueue<string> logQueue = new();
    private readonly SemaphoreSlim semaphore = new(1, 1);
    private Timer? timer;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonStorageLogger{T, C}"/> class and starts the periodic flush timer.
    /// </summary>
    /// <param name="azureStorageService">The storage service for persisting log events.</param>
    /// <param name="accessor">The HTTP context accessor for request metadata.</param>
    /// <param name="logger">The underlying console logger.</param>
    /// <param name="requestContextAccessor">The request context accessor for correlation data.</param>
    public JsonStorageLogger(AzureStorageServiceInternal<T, C> azureStorageService, IHttpContextAccessor accessor, ILogger<JsonLogger> logger, IRequestContextAccessor requestContextAccessor) {
        this.azureStorageService = azureStorageService;
        this.accessor = accessor;
        this.logger = logger;
        this.requestContextAccessor = requestContextAccessor;
        this.timer = new(TimeSpan.FromSeconds(30));
        this.timer.Elapsed += async (sender, args) => await this.FlushLogs()
            .ConfigureAwait(false);
        this.timer.Start();
    }

    /// <summary>
    /// Finalizes an instance of the <see cref="JsonStorageLogger{T, C}"/> class.
    /// </summary>
    ~JsonStorageLogger() {
        this.timer?.Stop();
        this.timer?.Dispose();
        this.timer = null;
    }

    /// <summary>
    /// Logs an error message with an associated exception.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public void Error(string message, Exception exception, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Error, message, exception, filePath, lineNumber, memberName, this.accessor, this.requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
        this.logQueue.Enqueue(logEvent);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public void Info(string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Information, message, null, filePath, lineNumber, memberName, this.accessor, this.requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
        this.logQueue.Enqueue(logEvent);
    }

    /// <summary>
    /// Logs a warning message with an optional exception.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="exception">An optional exception to log.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public void Warn(string message, Exception? exception = null, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Warning, message, exception, filePath, lineNumber, memberName, this.accessor, this.requestContextAccessor, latency);
        this.logger.LogWarning(logEvent);
        this.logQueue.Enqueue(logEvent);
    }

    /// <summary>
    /// Logs an error message if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public void IfError(bool condition, string message, Exception exception, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Error(message, exception, latency, filePath, lineNumber, memberName);
        }
    }

    /// <summary>
    /// Logs an informational message if the specified condition is true.
    /// </summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="message">The log message.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public void IfInfo(bool condition, string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Info(message, latency, filePath, lineNumber, memberName);
        }
    }

    private static string GetContainerAndBlobNames() =>
        $"{DateTime.UtcNow:yyyy-MM-dd}/logs.jsonl";

    private async ValueTask FlushLogs() {
        if (this.logQueue.IsEmpty) {
            return;
        }

        await this.semaphore.WaitAsync()
            .ConfigureAwait(false);

        string blobName = GetContainerAndBlobNames();

        try {
            StringBuilder sb = new();
            int count = 0;
            while (this.logQueue.TryDequeue(out string? events) && count < MaxLogQueueSize) {
                sb.AppendLine(events);
                count++;
            }

            using MemoryStream stream = new(Encoding.UTF8.GetBytes(sb.ToString()));

            try {
                await this.azureStorageService.AppendAsync(this.azureStorageService.Config.ContainerName, blobName, stream, CancellationToken.None)
                    .ConfigureAwait(false);
            } catch (Exception) {
                // Single retry with fresh stream position
                stream.Position = 0;
                await this.azureStorageService.AppendAsync(this.azureStorageService.Config.ContainerName, blobName, stream, CancellationToken.None)
                    .ConfigureAwait(false);
            }
        } catch (Exception ex) {
            Console.WriteLine($"Failed to flush {this.logQueue.Count} log events to storage after retry: {ex.Message}");
        } finally {
            this.semaphore.Release();
        }
    }
}
