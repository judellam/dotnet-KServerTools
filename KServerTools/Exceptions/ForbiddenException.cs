namespace KServerTools.Common;

/// <summary>
/// Exception thrown when the caller does not have permission to access the resource.
/// </summary>
public class ForbiddenException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ForbiddenException(string message) : base(ServiceError.Forbidden, message) { }
}
