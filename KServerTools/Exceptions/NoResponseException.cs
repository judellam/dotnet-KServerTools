namespace KServerTools.Common;

/// <summary>
/// Exception thrown when no response was received from the downstream service.
/// </summary>
public class NoResponseException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="NoResponseException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public NoResponseException(string message) : base(ServiceError.NoResponse, message) { }
}
