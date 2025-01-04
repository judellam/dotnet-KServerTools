namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;

static class Extensions {
    public static Guid TryOrMakeGuid(this HttpContext context, string headerName) {
        string rid = context.Request.Headers[headerName].ToString();
        if (Guid.TryParse(rid, out Guid requestId)) {
            return requestId;
        } else {
            return Guid.NewGuid();
        }
    }
}