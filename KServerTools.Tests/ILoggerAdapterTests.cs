namespace KServerTools.Tests;

using KServerTools.Common;
using Microsoft.Extensions.Logging;
using Moq;

public class ILoggerAdapterTests {
    [Fact]
    public void Info_DelegatesToILogger() {
        var mockLogger = new Mock<ILogger<ILoggerAdapterTests>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var adapter = new ILoggerAdapter<ILoggerAdapterTests>(mockLogger.Object);
        adapter.Info("test message", 42);

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Error_DelegatesToILoggerWithException() {
        var mockLogger = new Mock<ILogger<ILoggerAdapterTests>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var exception = new InvalidOperationException("boom");

        var adapter = new ILoggerAdapter<ILoggerAdapterTests>(mockLogger.Object);
        adapter.Error("error message", exception, 100);

        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Warn_DelegatesToILogger() {
        var mockLogger = new Mock<ILogger<ILoggerAdapterTests>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Warning)).Returns(true);

        var adapter = new ILoggerAdapter<ILoggerAdapterTests>(mockLogger.Object);
        adapter.Warn("warn message");

        mockLogger.Verify(l => l.Log(
            LogLevel.Warning,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void Info_SkipsWhenLogLevelDisabled() {
        var mockLogger = new Mock<ILogger<ILoggerAdapterTests>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(false);

        var adapter = new ILoggerAdapter<ILoggerAdapterTests>(mockLogger.Object);
        adapter.Info("should not log");

        mockLogger.Verify(l => l.Log(
            It.IsAny<LogLevel>(),
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            It.IsAny<Exception?>(),
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Never);
    }

    [Fact]
    public void IfInfo_OnlyLogsWhenConditionTrue() {
        var mockLogger = new Mock<ILogger<ILoggerAdapterTests>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Information)).Returns(true);

        var adapter = new ILoggerAdapter<ILoggerAdapterTests>(mockLogger.Object);
        adapter.IfInfo(false, "should not log");
        adapter.IfInfo(true, "should log");

        mockLogger.Verify(l => l.Log(
            LogLevel.Information,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            null,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }

    [Fact]
    public void IfError_OnlyLogsWhenConditionTrue() {
        var mockLogger = new Mock<ILogger<ILoggerAdapterTests>>();
        mockLogger.Setup(l => l.IsEnabled(LogLevel.Error)).Returns(true);
        var ex = new Exception("test");

        var adapter = new ILoggerAdapter<ILoggerAdapterTests>(mockLogger.Object);
        adapter.IfError(false, "nope", ex);
        adapter.IfError(true, "yes", ex);

        mockLogger.Verify(l => l.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.IsAny<It.IsAnyType>(),
            ex,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
    }
}
