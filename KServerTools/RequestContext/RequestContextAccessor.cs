namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;

internal class RequestContextAccessor<T>(IHttpContextAccessor httpContextAccessor) : IRequestContextAccessor where T : class, IRequestContext, new() {
    private readonly IHttpContextAccessor httpContextAccessor = httpContextAccessor;

    /// <summary>
    /// Cache for the request context. Scoped to the request thread.
    /// </summary>
    private readonly AsyncLocal<T> requestContextCache = new();

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