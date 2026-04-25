namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Provides access to the current request context, caching it per async flow.
/// </summary>
/// <typeparam name="T">The request context type implementing <see cref="IRequestContext"/>.</typeparam>
/// <param name="httpContextAccessor">The HTTP context accessor.</param>
internal class RequestContextAccessor<T>(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor where T : class, IRequestContext, new() {
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Cache for the request context. Scoped to the request thread.
    /// </summary>
    private readonly AsyncLocal<T> requestContextCache = new();

    /// <summary>
    /// Gets the current request context, creating and caching it if necessary.
    /// </summary>
    /// <returns>The current <see cref="IRequestContext"/>, or <see langword="null"/> if no HTTP context is available.</returns>
    public IRequestContext? GetRequestContext() {
        if (this.httpContextAccessor == null || this.httpContextAccessor.HttpContext == null) {
            return null;
        }

        if (this.requestContextCache.Value != null) {
            return this.requestContextCache.Value;
        }

        T requestContext = new();
        requestContext.Setup(this.httpContextAccessor.HttpContext);
        this.requestContextCache.Value = requestContext;

        return requestContext;
    }
}
