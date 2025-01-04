namespace KServerTools.Common;

internal class LogEvent {
    public required string Level { get; set; }
    public required string Message { get; set; }
    public long? Latency { get; set; } = null;
    public string? ExceptionMessage { get; set; } = null;
    public string? ExceptionType { get; set; } = null;
    public required string RequestId { get; set; }
    public string? Url { get; set; } = null;
    public required string Method { get; set; }
    public string? StatusCode { get; set; } = null;
    public required string FilePath { get; set; }
    public required string MemberName { get; set; }
    public required string LineNumber { get; set; }
}