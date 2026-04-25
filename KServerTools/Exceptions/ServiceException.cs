namespace KServerTools.Common;

/// <summary>
/// Base exception for service-layer errors, carrying a <see cref="ServiceError"/> code.
/// </summary>
public class ServiceException : Exception {
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a service error code and message.
    /// </summary>
    /// <param name="serviceError">The service error code.</param>
    /// <param name="message">The error message.</param>
    public ServiceException(
        ServiceError serviceError,
        string message) : base(message) {
        this.ServiceError = serviceError;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceException"/> class with a service error code, message, and inner exception.
    /// </summary>
    /// <param name="serviceError">The service error code.</param>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The inner exception.</param>
    public ServiceException(
        ServiceError serviceError,
        string message,
        Exception exception) : base(message, exception) {
        this.ServiceError = serviceError;
    }

    /// <summary>
    /// Gets the service error code associated with this exception.
    /// </summary>
    public ServiceError ServiceError { get; }
}
