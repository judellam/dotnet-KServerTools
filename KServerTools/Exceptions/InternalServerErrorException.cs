namespace KServerTools.Common;

/// <summary>
/// Exception thrown when an internal server error occurs.
/// </summary>
public class InternalServerErrorException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public InternalServerErrorException(string message) : base(ServiceError.InternalServerError, message) { }

    /// <summary>
    /// Initializes a new instance of the <see cref="InternalServerErrorException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="exception">The inner exception.</param>
    public InternalServerErrorException(string message, Exception exception) : base(ServiceError.InternalServerError, message, exception) { }

    /// <summary>
    /// Throws an <see cref="InternalServerErrorException"/> if the specified object is null.
    /// </summary>
    /// <param name="o">The object to check.</param>
    /// <param name="message">The error message to include if the object is null.</param>
    /// <returns>The object if it is not null; otherwise throws.</returns>
    public static object? ThrowIfArgumentIsNull(object? o, string message = "") => o ?? throw new InternalServerErrorException(message ?? "Internal Server Error");
}
