namespace KServerTools.Common;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

/// <summary>
/// Utility methods for creating structured JSON log events.
/// </summary>
internal static class LoggingUtilities {
    private static readonly JsonSerializerOptions LoggingSerializationOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Creates a serialized JSON log event string from the provided parameters.
    /// </summary>
    /// <param name="logLevel">The severity level of the log event.</param>
    /// <param name="message">The log message.</param>
    /// <param name="exception">An optional exception associated with the event.</param>
    /// <param name="filePath">The source file path where the event originated.</param>
    /// <param name="lineNumber">The source line number where the event originated.</param>
    /// <param name="memberName">The member name where the event originated.</param>
    /// <param name="accessor">The HTTP context accessor for request metadata.</param>
    /// <param name="requestContextAccessor">The request context accessor for correlation data.</param>
    /// <param name="latency">Optional operation latency in milliseconds.</param>
    /// <returns>A JSON-serialized string representing the log event.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string GetLogEvent(LogLevel logLevel, string message, Exception? exception, string filePath, int lineNumber, string memberName, IHttpContextAccessor accessor, IRequestContextAccessor requestContextAccessor, long? latency = null) {
        IRequestContext? requestContext = requestContextAccessor.GetRequestContext();
        LogEvent logEvent = new() {
            UserAgent = requestContext?.UserAgent ?? string.Empty,
            Message = message,
            Level = logLevel.ToString(),
            ExceptionType = exception?.GetType().ToString() ?? null,
            ExceptionMessage = exception?.Message ?? null,
            RequestId = requestContext?.RequestId.ToString() ?? Guid.Empty.ToString(),
            Url = accessor?.HttpContext?.Request.Path ?? null,
            Method = accessor?.HttpContext?.Request.Method ?? string.Empty,
            StatusCode = accessor?.HttpContext?.Response.StatusCode.ToString() ?? null,
            FilePath = Path.GetFileName(filePath),
            LineNumber = lineNumber.ToString(),
            MemberName = memberName,
            Latency = latency
        };

        return JsonSerializer.Serialize(
            logEvent,
            LoggingSerializationOptions);
    }
}
