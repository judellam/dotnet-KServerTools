namespace KServerTools.Common;

/// <summary>
/// Exception thrown when the request is invalid or malformed.
/// </summary>
public class BadRequestException : ServiceException {
    /// <summary>
    /// Initializes a new instance of the <see cref="BadRequestException"/> class.
    /// </summary>
    /// <param name="message">The error message.</param>
    public BadRequestException(string message) : base(ServiceError.BadRequest, message) { }

    /// <summary>
    /// Throws a <see cref="BadRequestException"/> if the specified object is null.
    /// </summary>
    /// <param name="o">The object to check.</param>
    /// <param name="argument">The argument name to include in the error message.</param>
    /// <returns>The non-null object.</returns>
    public static object ThrowIfArgumentIsNull(object? o, string argument) => o ?? throw new BadRequestException($"{argument} cannot be null");
}
