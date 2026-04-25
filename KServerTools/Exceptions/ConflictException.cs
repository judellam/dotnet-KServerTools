namespace KServerTools.Common;

/// <summary>
/// Exception thrown when a resource conflict occurs (e.g., duplicate creation).
/// </summary>
public class ConflictException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public ConflictException(string message) : base(ServiceError.Conflict, message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="ex">The inner exception that caused the conflict.</param>
    public ConflictException(string message, Exception ex) : base(ServiceError.Conflict, message, ex) { }
}
