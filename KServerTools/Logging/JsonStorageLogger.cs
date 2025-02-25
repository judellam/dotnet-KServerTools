namespace KServerTools.Common;

using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Text;
using System.Timers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal class JsonStorageLogger<T, C> : IJsonLogger where T : AzureStorageServiceLogConfig where C : ITokenCredentialService {
    private readonly AzureStorageServiceInternal<T, C> azureStorageService;
    private readonly IHttpContextAccessor accessor;
    private readonly ILogger<JsonLogger> logger;
    private readonly IRequestContextAccessor requestContextAccessor;
    private readonly ConcurrentQueue<string> logQueue = new();
    private Timer? timer;
    private const int MaxLogQueueSize = 1000;
    private SemaphoreSlim semaphore = new(1, 1);

    public JsonStorageLogger(AzureStorageServiceInternal<T, C> azureStorageService, IHttpContextAccessor accessor, ILogger<JsonLogger> logger, IRequestContextAccessor requestContextAccessor) {
        this.azureStorageService = azureStorageService;
        this.accessor = accessor;
        this.logger = logger;
        this.requestContextAccessor = requestContextAccessor;
        this.timer = new(TimeSpan.FromSeconds(30));
        timer.Elapsed += async (sender, args) => await this.FlushLogs()
            .ConfigureAwait(false);
        this.timer.Start();
    }

    ~JsonStorageLogger() {
        this.timer?.Stop();
        this.timer?.Dispose();
        this.timer = null;
    }

    public void Error(string message, Exception exception, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Error, message, exception, filePath, lineNumber, memberName, accessor, requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
        this.logQueue.Enqueue(logEvent);
    }

    public void Info(string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Information, message, null, filePath, lineNumber, memberName, accessor, requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
        this.logQueue.Enqueue(logEvent);
    }

    public void Warn(string message, Exception? exception = null, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Warning, message, exception, filePath, lineNumber, memberName, accessor, requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
        this.logQueue.Enqueue(logEvent);
    }

    public void IfError(bool condition, string message, Exception exception, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Error(message, exception, latency, filePath, lineNumber, memberName);
        }
    }

    public void IfInfo(bool condition, string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Info(message, latency, filePath, lineNumber, memberName);
        }
    }

    private async ValueTask FlushLogs() {
        if (this.logQueue.IsEmpty) {
            return;
        }

        await this.semaphore.WaitAsync()
            .ConfigureAwait(false);

        string blobName = GetContainerAndBlobNames();

        try {
            StringBuilder sb = new();
            string events = string.Empty;
            int count = 0;
            while (this.logQueue.TryDequeue(out events!) && count < MaxLogQueueSize) {
                sb.AppendLine(events);
            }
            using MemoryStream stream = new(Encoding.UTF8.GetBytes(sb.ToString()));

            await this.azureStorageService.AppendAsync(this.azureStorageService.Config.ContainerName, blobName, stream, CancellationToken.None)
                .ConfigureAwait(false);
        } catch (Exception ex){ 
            Console.WriteLine($"Failed to flush logs to storage: {ex.Message}, {ex.StackTrace}");
        } finally {
            this.semaphore.Release();
        }
    }

    private static string GetContainerAndBlobNames() => 
        $"{DateTime.UtcNow:yyyy-MM-dd}/logs.jsonl";
}