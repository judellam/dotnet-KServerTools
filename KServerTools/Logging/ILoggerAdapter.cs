namespace KServerTools.Common;

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

/// <summary>
/// Adapter that bridges Microsoft.Extensions.Logging.ILogger to IJsonLogger.
/// Allows consumers to use the standard ILogger infrastructure while keeping
/// KServerTools' structured JSON logging format.
/// </summary>
internal class ILoggerAdapter<T>(ILogger<T> logger, IRequestContextAccessor? requestContextAccessor = null, Microsoft.AspNetCore.Http.IHttpContextAccessor? httpContextAccessor = null) : IJsonLogger {

    public void Info(string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation("{Message} | {MemberName} ({FilePath}:{LineNumber}) | Latency={Latency}ms",
                message, memberName, Path.GetFileName(filePath), lineNumber, latency);
        }
    }

    public void Warn(string message, Exception? exception = null, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (logger.IsEnabled(LogLevel.Warning)) {
            logger.LogWarning(exception, "{Message} | {MemberName} ({FilePath}:{LineNumber}) | Latency={Latency}ms",
                message, memberName, Path.GetFileName(filePath), lineNumber, latency);
        }
    }

    public void Error(string message, Exception exception, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (logger.IsEnabled(LogLevel.Error)) {
            logger.LogError(exception, "{Message} | {MemberName} ({FilePath}:{LineNumber}) | Latency={Latency}ms",
                message, memberName, Path.GetFileName(filePath), lineNumber, latency);
        }
    }

    public void IfInfo(bool condition, string message, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Info(message, latency, filePath, lineNumber, memberName);
        }
    }

    public void IfError(bool condition, string message, Exception exception, long? latency = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0, [CallerMemberName] string memberName = "") {
        if (condition) {
            this.Error(message, exception, latency, filePath, lineNumber, memberName);
        }
    }
}
