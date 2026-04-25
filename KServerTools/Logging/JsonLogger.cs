namespace KServerTools.Common;

using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

// Future work:
// 1. Add support for additional loggers to be passed in to build a chain (ie: can we log to disk, azure storage, etc).
// 2. Allow for custom log events to be defined.

/// <summary>
/// Core JSON logger implementation that writes structured log events to the console.
/// </summary>
/// <param name="accessor">The HTTP context accessor for request metadata.</param>
/// <param name="logger">The underlying Microsoft logger.</param>
/// <param name="requestContextAccessor">The request context accessor for correlation data.</param>
internal class JsonLogger(IHttpContextAccessor accessor, ILogger<JsonLogger> logger, IRequestContextAccessor requestContextAccessor) : IJsonLogger {
    private readonly IHttpContextAccessor accessor = accessor;
    private readonly ILogger logger = logger;
    private readonly IRequestContextAccessor requestContextAccessor = requestContextAccessor;

    /// <summary>
    /// Logs an error message with an associated exception.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public virtual void Error(
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Error, message, exception, filePath, lineNumber, memberName, this.accessor, this.requestContextAccessor, latency);
        this.logger.LogError(logEvent);
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
    public virtual void Warn(
        string message,
        Exception? exception = null,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Warning, message, exception, filePath, lineNumber, memberName, this.accessor, this.requestContextAccessor, latency);
        this.logger.LogWarning(logEvent);
    }

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public virtual void Info(
        string message,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = LoggingUtilities.GetLogEvent(LogLevel.Information, message, null, filePath, lineNumber, memberName, this.accessor, this.requestContextAccessor, latency);
        this.logger.LogInformation(logEvent);
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
