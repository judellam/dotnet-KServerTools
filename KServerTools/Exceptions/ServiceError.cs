namespace KServerTools.Common;

/// <summary>
/// Represents an error code from a service. To be used with the ServiceException.
/// </summary>
public enum ServiceError {
    /// <summary>The requested resource was not found (HTTP 404).</summary>
    NotFound = 404,

    /// <summary>The caller is not authenticated (HTTP 401).</summary>
    Unauthorized = 401,

    /// <summary>The caller does not have permission (HTTP 403).</summary>
    Forbidden = 403,

    /// <summary>The request is invalid or malformed (HTTP 400).</summary>
    BadRequest = 400,

    /// <summary>No response was received from the downstream service (HTTP 444).</summary>
    NoResponse = 444,

    /// <summary>An internal server error occurred (HTTP 500).</summary>
    InternalServerError = 500,

    /// <summary>The service is temporarily unavailable (HTTP 503).</summary>
    ServiceUnavailable = 503,

    /// <summary>The gateway timed out waiting for a response (HTTP 504).</summary>
    GatewayTimeout = 504,

    /// <summary>A resource conflict occurred (HTTP 409).</summary>
    Conflict = 409,

    /// <summary>The error type is unknown or unclassified.</summary>
    Unknown = -1
}
