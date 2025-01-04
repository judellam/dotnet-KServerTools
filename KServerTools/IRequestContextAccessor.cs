namespace KServerTools.Common;

/// <summary>
/// The request context. It keeps state about the current request and parses the headers for useful information.
/// </summary>
public interface IRequestContextAccessor<T> where T: class, IRequestContext, new() {
    /// <summary>
    /// Retrieves the request context.
    /// </summary>
    public T? GetRequestContext();
}