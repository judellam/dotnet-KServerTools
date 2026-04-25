namespace KServerTools.Common;

/// <summary>
/// Exception thrown when the caller is not authenticated.
/// </summary>
public class UnauthorizedException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedException(string message) : base(ServiceError.Unauthorized, message) { }
}
