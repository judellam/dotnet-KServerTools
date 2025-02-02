namespace KServerTools.Common;

using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

internal static class LoggingUtilities {
    private static readonly JsonSerializerOptions LoggingSerializationOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

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
            FilePath = filePath,
            LineNumber = lineNumber.ToString(),
            MemberName = memberName,
            Latency = latency
        };

        return JsonSerializer.Serialize(
            logEvent, 
            LoggingSerializationOptions);
    }
}