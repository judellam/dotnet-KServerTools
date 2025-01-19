namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;

/// <summary>
/// The request context. It keeps state about the current request and parses the headers for useful information.
/// </summary>
public interface IRequestContext {
    /// <summary>
    /// The request id. The user can send this in, or will be generated if not present.
    /// </summary>
    public Guid RequestId { get; }

    /// <summary>
    /// The user agent of the request.
    /// </summary>
    public string? UserAgent { get; }

    /// <summary>
    /// Called for setup of the request context.
    /// </summary>
    /// <param name="context"></param>
    void Setup(HttpContext context);
}