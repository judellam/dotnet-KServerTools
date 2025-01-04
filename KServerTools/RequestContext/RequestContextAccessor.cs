using Microsoft.AspNetCore.Http;

namespace KServerTools.Common;

internal class RequestContextAccessor<T> : IRequestContextAccessor<T> where T : class, IRequestContext, new() {
    private readonly IHttpContextAccessor httpContextAccessor;

    private AsyncLocal<T> requestContextCache = new();

    public RequestContextAccessor(IHttpContextAccessor httpContextAccessor) {
        this.httpContextAccessor = httpContextAccessor;
    }

    public T? GetRequestContext() {
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