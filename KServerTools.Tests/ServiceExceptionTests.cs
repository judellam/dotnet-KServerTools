namespace KServerTools.Tests;

using KServerTools.Common;

public class ServiceExceptionTests {
    [Theory]
    [InlineData(typeof(NotFoundException), ServiceError.NotFound)]
    [InlineData(typeof(UnauthorizedException), ServiceError.Unauthorized)]
    [InlineData(typeof(ForbiddenException), ServiceError.Forbidden)]
    [InlineData(typeof(BadRequestException), ServiceError.BadRequest)]
    [InlineData(typeof(NoResponseException), ServiceError.NoResponse)]
    [InlineData(typeof(InternalServerErrorException), ServiceError.InternalServerError)]
    [InlineData(typeof(ConflictException), ServiceError.Conflict)]
    public void Exception_HasCorrectServiceError(Type exceptionType, ServiceError expectedError) {
        var ex = (ServiceException)Activator.CreateInstance(exceptionType, "test message")!;
        Assert.Equal(expectedError, ex.ServiceError);
        Assert.Equal("test message", ex.Message);
    }

    [Fact]
    public void BadRequestException_ThrowIfArgumentIsNull_ThrowsOnNull() {
        Assert.Throws<BadRequestException>(() => BadRequestException.ThrowIfArgumentIsNull(null, "myArg"));
    }

    [Fact]
    public void BadRequestException_ThrowIfArgumentIsNull_ReturnsValueWhenNotNull() {
        var obj = new object();
        var result = BadRequestException.ThrowIfArgumentIsNull(obj, "myArg");
        Assert.Same(obj, result);
    }

    [Fact]
    public void InternalServerErrorException_ThrowIfArgumentIsNull_ThrowsOnNull() {
        Assert.Throws<InternalServerErrorException>(() => InternalServerErrorException.ThrowIfArgumentIsNull(null, "server error"));
    }

    [Fact]
    public void InternalServerErrorException_WithInnerException() {
        var inner = new InvalidOperationException("inner");
        var ex = new InternalServerErrorException("outer", inner);
        Assert.Equal("outer", ex.Message);
        Assert.Same(inner, ex.InnerException);
    }
}
