namespace KServerTools.Common;

/// <summary>
/// Exception thrown when access is unauthorized.
/// </summary>
public class UnauthorizedAccessException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="UnauthorizedAccessException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public UnauthorizedAccessException(string message) : base(ServiceError.Unauthorized, message) { }
}
