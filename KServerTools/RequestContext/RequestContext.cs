namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;

internal static class RequestContextConstants {
    public static class Headers {
        public const string Authorization = "Authorization";
        public const string SessionToken = "x-session-token";
        public const string UserName = "x-user-name";
        public const string RequestId = "x-request-id";
    }
}

internal class RequestContext : IRequestContext {

    public Guid RequestId { get; private set; }

    public RequestContext() {
    }

    public void Setup(HttpContext context) {
        this.RequestId = context.TryOrMakeGuid(RequestContextConstants.Headers.RequestId);
    }
}