namespace KServerTools.Common;

/// <summary>
/// Represents a structured log event for JSON serialization.
/// </summary>
internal class LogEvent {
    /// <summary>
    /// Gets or sets the UTC timestamp when the log event occurred.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Gets or sets the user agent string from the originating request.
    /// </summary>
    public required string UserAgent { get; set; }

    /// <summary>
    /// Gets or sets the log level (e.g., Information, Warning, Error).
    /// </summary>
    public required string Level { get; set; }

    /// <summary>
    /// Gets or sets the log message.
    /// </summary>
    public required string Message { get; set; }

    /// <summary>
    /// Gets or sets the operation latency in milliseconds, if applicable.
    /// </summary>
    public long? Latency { get; set; } = null;

    /// <summary>
    /// Gets or sets the exception message, if an exception was logged.
    /// </summary>
    public string? ExceptionMessage { get; set; } = null;

    /// <summary>
    /// Gets or sets the exception type name, if an exception was logged.
    /// </summary>
    public string? ExceptionType { get; set; } = null;

    /// <summary>
    /// Gets or sets the request identifier for correlation.
    /// </summary>
    public required string RequestId { get; set; }

    /// <summary>
    /// Gets or sets the request URL path, if available.
    /// </summary>
    public string? Url { get; set; } = null;

    /// <summary>
    /// Gets or sets the HTTP method of the request.
    /// </summary>
    public required string Method { get; set; }

    /// <summary>
    /// Gets or sets the HTTP status code of the response, if available.
    /// </summary>
    public string? StatusCode { get; set; } = null;

    /// <summary>
    /// Gets or sets the source file path where the log event originated.
    /// </summary>
    public required string FilePath { get; set; }

    /// <summary>
    /// Gets or sets the member name where the log event originated.
    /// </summary>
    public required string MemberName { get; set; }

    /// <summary>
    /// Gets or sets the source line number where the log event originated.
    /// </summary>
    public required string LineNumber { get; set; }
}
