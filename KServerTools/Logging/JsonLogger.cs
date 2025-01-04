namespace KServerTools.Common;

using System.Text.Json;
using System.Text.Json.Serialization;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;

// Future work:
// 1. Add support for additional loggers to be passed in to build a chain (ie: can we log to disk, azure storage, etc).
// 2. Add support for conditional logging (ie: if something meets a certain criteria, log it).
// 3. Allow for custom log events to be defined.
internal class JsonLogger(
    IHttpContextAccessor accessor,
    ILogger<JsonLogger> logger,
    IRequestContextAccessor<RequestContext> requestContextAccessor) : IJsonLogger {
    private readonly IHttpContextAccessor accessor = accessor;
    private readonly ILogger logger = logger;
    private readonly IRequestContextAccessor<RequestContext> requestContextAccessor = requestContextAccessor;
    private static readonly JsonSerializerOptions LoggingSerializationOptions = new() {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public void Error(
        string message,
        Exception exception,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = GetLogEvent(LogLevel.Error, message, exception, filePath, lineNumber, memberName, latency);
        this.logger.LogInformation(logEvent);
    }

    public void Warn(
        string message,
        Exception? exception = null,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = GetLogEvent(LogLevel.Warning, message, exception, filePath, lineNumber, memberName, latency);
        this.logger.LogInformation(logEvent);
    }

    public void Info(
        string message,
        long? latency = null,
        [CallerFilePath] string filePath = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerMemberName] string memberName = "") {
        string logEvent = GetLogEvent(LogLevel.Information, message, null, filePath, lineNumber, memberName, latency);
        this.logger.LogInformation(logEvent);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private string GetLogEvent(LogLevel logLevel, string message, Exception? exception, string filePath, int lineNumber, string memberName, long? latency = null) {
        IRequestContext? requestContext = this.requestContextAccessor.GetRequestContext();
        LogEvent logEvent = new() {
            Message = message,
            Level = logLevel.ToString(),
            ExceptionType = exception?.GetType().ToString() ?? null,
            ExceptionMessage = exception?.Message ?? null,
            RequestId = requestContext?.RequestId.ToString() ?? Guid.Empty.ToString(),
            Url = this.accessor?.HttpContext?.Request.Path ?? null,
            Method = this.accessor?.HttpContext?.Request.Method ?? string.Empty,
            StatusCode = this.accessor?.HttpContext?.Response.StatusCode.ToString() ?? null,
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