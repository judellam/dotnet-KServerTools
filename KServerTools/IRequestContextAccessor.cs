namespace KServerTools.Common;

/// <summary>
/// The request context. It keeps state about the current request and parses the headers for useful information.
/// </summary>
public interface IRequestContextAccessor {
    /// <summary>
    /// Retrieves the request context.
    /// </summary>
    public IRequestContext? GetRequestContext();
}