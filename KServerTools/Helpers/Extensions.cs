namespace KServerTools.Common;

using Microsoft.AspNetCore.Http;

/// <summary>
/// Extension methods for HTTP context operations.
/// </summary>
internal static class Extensions {
    /// <summary>
    /// Attempts to parse a GUID from the specified request header, or creates a new one if parsing fails.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="headerName">The name of the header to extract the GUID from.</param>
    /// <returns>The parsed or newly generated <see cref="Guid"/>.</returns>
    public static Guid TryOrMakeGuid(this HttpContext context, string headerName) {
        string rid = context.Request.Headers[headerName].ToString();
        if (Guid.TryParse(rid, out Guid requestId)) {
            return requestId;
        } else {
            return Guid.NewGuid();
        }
    }
}
