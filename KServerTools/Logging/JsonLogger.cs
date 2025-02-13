namespace KServerTools.Common;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

// Future work:
// 1. Add support for additional loggers to be passed in to build a chain (ie: can we log to disk, azure storage, etc).
// 2. Allow for custom log events to be defined.
internal class JsonLogger(IHttpContextAccessor accessor, ILogger<JsonLogger> logger, IRequestContextAccessor requestContextAccessor) : IJsonLogger {
    private readonly IHttpContextAccessor accessor = accessor;
    private readonly ILogger logger = logger;
    private readonly IRequestContextAccessor requestContextAccessor = requestContextAccessor;

    public virtual void Error(
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Error, message, exception, filePath, lineNumber, memberName, accessor, requestContextAccessor, latency);
        this.logger.LogError(logEvent);
    }

    public virtual void Warn(
        string message,
        Exception? exception = null,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Warning, message, exception, filePath, lineNumber, memberName, accessor, requestContextAccessor, latency);
        this.logger.LogWarning(logEvent);
    }

    public virtual void Info(
        string message,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Information, message, null, filePath, lineNumber, memberName, accessor, requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
    }

    public void IfInfo(
        bool condition,
        string message,
        long? latency = null, 
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Info(message, latency, filePath, lineNumber, memberName);
        }
    }    

    public void IfError(
        bool condition,
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Error(message, exception, latency, filePath, lineNumber, memberName);
        }
    }
}