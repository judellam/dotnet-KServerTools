namespace KServerTools.Common;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
public class NotFoundException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NotFoundException(string message) : base(ServiceError.NotFound, message) { }
}
