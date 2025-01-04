namespace KServerTools.Common;

/// <summary>
/// Represents an error code from a service. To be used with the ServiceException.
/// </summary>
public enum ServiceError {
    NotFound = 404,
    Unauthorized = 401,
    Forbidden = 403,
    BadRequest = 400,
    NoResponse = 444,
    InternalServerError = 500,
    ServiceUnavailable = 503,
    GatewayTimeout = 504,
    Conflict = 409,
    Unknown = -1
}