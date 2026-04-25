namespace KServerTools.Common;

/// <summary>
/// Identifies who initiated the cancellation of an async operation.
/// </summary>
public enum CancellationSource {
    /// <summary>The caller cancelled the request (e.g., client disconnect, HttpContext.RequestAborted).</summary>
    Caller,
    /// <summary>The server cancelled the request (e.g., internal timeout, app shutdown).</summary>
    Server,
    /// <summary>No token context available to determine the source.</summary>
    Unknown
}
