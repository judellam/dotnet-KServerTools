namespace KServerTools.Common;

public class ServiceException : Exception {
    public ServiceException(
        ServiceError serviceError, 
        string message) : base(message) {
        this.ServiceError = serviceError;
    }
    public ServiceException(
        ServiceError serviceError, 
        string message,
        Exception exception) : base(message, exception) {
        this.ServiceError = serviceError;
    }


    public ServiceError ServiceError { get; }
}

public class NotFoundException : ServiceException {
    public NotFoundException(string message) : base(ServiceError.NotFound, message) { }
}

public class UnauthorizedException : ServiceException {
    public UnauthorizedException(string message) : base(ServiceError.Unauthorized, message) { }
}

public class ForbiddenException : ServiceException {
    public ForbiddenException(string message) : base(ServiceError.Forbidden, message) { }
}

public class NoResponseException : ServiceException {
    public NoResponseException(string message) : base(ServiceError.NoResponse, message) { }
}

public class BadRequestException : ServiceException {
    public BadRequestException(string message) : base(ServiceError.BadRequest, message) { }
    static public object ThrowIfArgumentIsNull(object? o, string argument) => o ?? throw new BadRequestException($"{argument} cannot be null");
}

public class InternalServerErrorException : ServiceException {
    public InternalServerErrorException(string message) : base(ServiceError.InternalServerError, message) { }
    public InternalServerErrorException(string message, Exception exception) : base(ServiceError.InternalServerError, message, exception) { }
    public static object? ThrowIfArgumentIsNull(object? o, string argument, string message = "") => o ?? throw new InternalServerErrorException(message ?? "Internal Server Error");
}

public class ConflictException : ServiceException {
    public ConflictException(string message) : base(ServiceError.Conflict, message) { }
    public ConflictException(string message, Exception ex) : base(ServiceError.Conflict, message) { }
}

public class UnauthorizedAccessException : ServiceException {
    public UnauthorizedAccessException(string message) : base(ServiceError.Unauthorized, message) { }
}