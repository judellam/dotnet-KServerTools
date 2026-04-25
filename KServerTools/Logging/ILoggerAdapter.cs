namespace KServerTools.Common;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Adapter that bridges Microsoft.Extensions.Logging.ILogger to IJsonLogger.
/// Allows consumers to use the standard ILogger infrastructure while keeping
/// KServerTools' structured JSON logging format.
/// </summary>
/// <typeparam name="T">The category type for the underlying <see cref="ILogger{T}"/>.</typeparam>
/// <param name="logger">The underlying Microsoft logger.</param>
/// <param name="requestContextAccessor">An optional request context accessor (reserved for future use).</param>
/// <param name="httpContextAccessor">An optional HTTP context accessor (reserved for future use).</param>
internal class ILoggerAdapter<T>(ILogger<T> logger, IRequestContextAccessor? requestContextAccessor = null, Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null) : IJsonLogger {
    private readonly IRequestContextAccessor? requestContextAccessor = requestContextAccessor;
    private readonly Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Logs an informational message.
    /// </summary>
    /// <param name="message">The log message.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <param name="filePath">The source file path (auto-populated).</param>
    /// <param name="lineNumber">The source line number (auto-populated).</param>
    /// <param name="memberName">The calling member name (auto-populated).</param>
    public void Info(string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation("{Message} | {MemberName} ({FilePath}:{LineNumber}) | Latency={Latency}ms",
                message, memberName, Path.GetFileName(filePath), lineNumber, latency);
        }
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
        if (logger.IsEnabled(LogLevel.Warning)) {
            logger.LogWarning(exception, "{Message} | {MemberName} ({FilePath}:{LineNumber}) | Latency={Latency}ms",
                message, memberName, Path.GetFileName(filePath), lineNumber, latency);
        }
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
        if (logger.IsEnabled(LogLevel.Error)) {
            logger.LogError(exception, "{Message} | {MemberName} ({FilePath}:{LineNumber}) | Latency={Latency}ms",
                message, memberName, Path.GetFileName(filePath), lineNumber, latency);
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
}
