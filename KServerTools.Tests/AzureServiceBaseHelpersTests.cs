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
}
