namespace KServerTools.Tests;

using KServerTools.Common;
using Moq;

public class AzureServiceBaseHelpersTests {
    [Fact]
    public async Task LoggedOperationAsync_Void_LogsSuccessWithLatency() {
        var logger = new Mock<IJsonLogger>();

        await AzureServiceBaseHelpers.LoggedOperationAsync(logger.Object, "upload-blob", () => Task.CompletedTask);

        logger.Verify(l => l.Info("upload-blob", It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_Void_LogsErrorAndRethrows() {
        var logger = new Mock<IJsonLogger>();
        var expectedException = new IOException("disk full");

        var ex = await Assert.ThrowsAsync<IOException>(
            () => AzureServiceBaseHelpers.LoggedOperationAsync(logger.Object, "write-blob", () => throw expectedException));

        Assert.Same(expectedException, ex);
        logger.Verify(l => l.Error(
            It.Is<string>(s => s.Contains("write-blob")),
            expectedException,
            It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_Generic_ReturnsValueAndLogs() {
        var logger = new Mock<IJsonLogger>();

        var result = await AzureServiceBaseHelpers.LoggedOperationAsync(logger.Object, "download-blob",
            () => Task.FromResult(new MemoryStream() as Stream));

        Assert.NotNull(result);
        logger.Verify(l => l.Info("download-blob", It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_Generic_LogsErrorAndRethrows() {
        var logger = new Mock<IJsonLogger>();
        var expectedException = new UnauthorizedAccessException("no access");

        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => AzureServiceBaseHelpers.LoggedOperationAsync<Stream>(logger.Object, "read-secret",
                () => throw expectedException));

        Assert.Same(expectedException, ex);
        logger.Verify(l => l.Error(
            It.Is<string>(s => s.Contains("read-secret")),
            expectedException,
            It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_Void_CallerCancellation_LogsCallerSource() {
        var logger = new Mock<IJsonLogger>();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => AzureServiceBaseHelpers.LoggedOperationAsync(logger.Object, "cancelled-upload", () => { cts.Token.ThrowIfCancellationRequested(); return Task.CompletedTask; }, cts.Token));

        logger.Verify(l => l.Warn(
            It.Is<string>(s => s.Contains("Cancelled (caller)")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        logger.Verify(l => l.Error(
            It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoggedOperationAsync_Generic_ServerCancellation_LogsServerSource() {
        var logger = new Mock<IJsonLogger>();
        using var callerCts = new CancellationTokenSource();
        using var serverCts = new CancellationTokenSource();
        serverCts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => AzureServiceBaseHelpers.LoggedOperationAsync<int>(logger.Object, "server-cancel",
                () => { serverCts.Token.ThrowIfCancellationRequested(); return Task.FromResult(0); }, callerCts.Token));

        logger.Verify(l => l.Warn(
            It.Is<string>(s => s.Contains("Cancelled (server)")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }
}
