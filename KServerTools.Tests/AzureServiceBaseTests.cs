namespace KServerTools.Tests;

using KServerTools.Common;
using Microsoft.Extensions.Caching.Memory;
using Moq;

public class AzureServiceBaseTests {
    // --- LoggedOperationAsync<T> ---

    [Fact]
    public async Task LoggedOperationAsync_ReturnsValueAndLogsSuccess() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        var result = await service.LoggedOperationAsync("get-item", () => Task.FromResult(42));

        Assert.Equal(42, result);
        logger.Verify(l => l.Info("get-item", It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_LogsErrorMessageWithOperationName() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoggedOperationAsync<int>("fetch-user", () => throw new InvalidOperationException("boom")));

        logger.Verify(l => l.Error(
            It.Is<string>(s => s.Contains("fetch-user")),
            It.IsAny<Exception>(),
            It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_RethrowsOriginalExceptionType() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1");

        var ex = await Assert.ThrowsAsync<ArgumentException>(
            () => service.LoggedOperationAsync<int>("op", () => throw new ArgumentException("bad arg")));

        Assert.Equal("bad arg", ex.Message);
    }

    // --- LoggedOperationAsync (void) ---

    [Fact]
    public async Task LoggedOperationAsync_Void_ExecutesAndLogs() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        bool executed = false;
        await service.LoggedOperationAsync("write-op", () => {
            executed = true;
            return Task.CompletedTask;
        });

        Assert.True(executed);
        logger.Verify(l => l.Info("write-op", It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_Void_LogsErrorAndRethrows() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        await Assert.ThrowsAsync<TimeoutException>(
            () => service.LoggedOperationAsync("timeout-op", () => throw new TimeoutException("timed out")));

        logger.Verify(l => l.Error(
            It.Is<string>(s => s.Contains("timeout-op")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_NullLogger_DoesNotThrow() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger: null);

        var result = await service.LoggedOperationAsync("safe-op", () => Task.FromResult(99));
        Assert.Equal(99, result);
    }

    [Fact]
    public async Task LoggedOperationAsync_IncludesPositiveLatency() {
        long? capturedLatency = null;
        var logger = new Mock<IJsonLogger>();
        logger.Setup(l => l.Info(It.IsAny<string>(), It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()))
            .Callback<string, long?, string, int, string>((_, latency, _, _, _) => capturedLatency = latency);
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        await service.LoggedOperationAsync("timed-op", async () => {
            await Task.Delay(10);
            return 1;
        });

        Assert.NotNull(capturedLatency);
        Assert.True(capturedLatency >= 0, $"Expected non-negative latency, got {capturedLatency}");
    }

    // --- GetOrCreateCachedAsync ---

    [Fact]
    public async Task GetOrCreateCachedAsync_ReturnsCachedValueOnSecondCall() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1");

        int factoryCalls = 0;
        var first = await service.GetOrCreateCachedAsync("key1", () => {
            factoryCalls++;
            return Task.FromResult("value1");
        });
        var second = await service.GetOrCreateCachedAsync("key1", () => {
            factoryCalls++;
            return Task.FromResult("value2");
        });

        Assert.Equal("value1", first);
        Assert.Equal("value1", second);
        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateCachedAsync_IsolatesByCredentialId() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var tenantA = new TestService(new TestConfig(), cache, "tenant-a");
        var tenantB = new TestService(new TestConfig(), cache, "tenant-b");

        var resultA = await tenantA.GetOrCreateCachedAsync("shared-key", () => Task.FromResult("from-a"));
        var resultB = await tenantB.GetOrCreateCachedAsync("shared-key", () => Task.FromResult("from-b"));

        Assert.Equal("from-a", resultA);
        Assert.Equal("from-b", resultB);
    }

    [Fact]
    public async Task GetOrCreateCachedAsync_FactoryExceptionIsNotCached() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1");

        int callCount = 0;
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.GetOrCreateCachedAsync<string>("fail-key", () => {
                callCount++;
                throw new InvalidOperationException("factory failed");
            }));

        // Second call should invoke factory again since first failed
        var result = await service.GetOrCreateCachedAsync("fail-key", () => {
            callCount++;
            return Task.FromResult("recovered");
        });

        Assert.Equal("recovered", result);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task GetOrCreateCachedAsync_RespectsCustomCacheOptions() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1");

        var shortLived = new MemoryCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMilliseconds(50) };

        await service.GetOrCreateCachedAsync("expiring", () => Task.FromResult("first"), shortLived);
        await Task.Delay(100);

        int callCount = 0;
        var result = await service.GetOrCreateCachedAsync("expiring", () => {
            callCount++;
            return Task.FromResult("second");
        }, shortLived);

        Assert.Equal("second", result);
        Assert.Equal(1, callCount);
    }

    // --- VerifyArgs ---

    [Fact]
    public void VerifyArgs_NullString_ThrowsException() {
        // VerifyArgs iterates the params array; a null element causes NullReferenceException
        // in string.IsNullOrWhiteSpace — this documents actual behavior
        Assert.ThrowsAny<Exception>(() => TestService.VerifyArgs(null!));
    }

    [Fact]
    public void VerifyArgs_EmptyString_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => TestService.VerifyArgs(string.Empty));
    }

    [Fact]
    public void VerifyArgs_WhitespaceString_ThrowsArgumentException() {
        Assert.Throws<ArgumentException>(() => TestService.VerifyArgs("   "));
    }

    [Fact]
    public void VerifyArgs_ValidStrings_DoesNotThrow() {
        TestService.VerifyArgs("hello", "world");
    }

    [Fact]
    public void VerifyArgs_MixedArgs_ThrowsOnFirstInvalid() {
        Assert.Throws<ArgumentException>(() => TestService.VerifyArgs("valid", string.Empty, "also-valid"));
    }

    [Fact]
    public void VerifyArgs_NoArgs_DoesNotThrow() {
        TestService.VerifyArgs();
    }

    // --- Cancellation handling ---

    [Fact]
    public async Task LoggedOperationAsync_CallerCancellation_LogsCallerSource() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.LoggedOperationAsync<int>("cancelled-op", () => {
                cts.Token.ThrowIfCancellationRequested();
                return Task.FromResult(0);
            }, cts.Token));

        logger.Verify(l => l.Warn(
            It.Is<string>(s => s.Contains("Cancelled (Caller)") && s.Contains("cancelled-op")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        logger.Verify(l => l.Error(
            It.IsAny<string>(), It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task LoggedOperationAsync_Void_CallerCancellation_LogsCallerSource() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.LoggedOperationAsync("void-cancel", () => {
                cts.Token.ThrowIfCancellationRequested();
                return Task.CompletedTask;
            }, cts.Token));

        logger.Verify(l => l.Warn(
            It.Is<string>(s => s.Contains("Cancelled (Caller)")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_ServerCancellation_LogsServerSource() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);
        // Caller token is NOT cancelled — the exception comes from an internal/server source
        using var callerCts = new CancellationTokenSource();
        using var serverCts = new CancellationTokenSource();
        serverCts.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.LoggedOperationAsync<int>("server-cancel", () => {
                serverCts.Token.ThrowIfCancellationRequested();
                return Task.FromResult(0);
            }, callerCts.Token));

        logger.Verify(l => l.Warn(
            It.Is<string>(s => s.Contains("Cancelled (Server)")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_NoCancellationToken_LogsUnknownSource() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        await Assert.ThrowsAsync<TaskCanceledException>(
            () => service.LoggedOperationAsync<int>("no-token", () => throw new TaskCanceledException("cancelled")));

        logger.Verify(l => l.Warn(
            It.Is<string>(s => s.Contains("Cancelled (Unknown)")),
            It.IsAny<Exception>(), It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public void GetCancellationSource_CallerTokenCancelled_ReturnsCaller() {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var ex = new OperationCanceledException(cts.Token);
        Assert.Equal(CancellationSource.Caller, AzureServiceBase<TestConfig>.GetCancellationSource(ex, cts.Token));
    }

    [Fact]
    public void GetCancellationSource_DifferentTokenCancelled_ReturnsServer() {
        using var callerCts = new CancellationTokenSource();
        using var serverCts = new CancellationTokenSource();
        serverCts.Cancel();
        var ex = new OperationCanceledException(serverCts.Token);
        Assert.Equal(CancellationSource.Server, AzureServiceBase<TestConfig>.GetCancellationSource(ex, callerCts.Token));
    }

    [Fact]
    public void GetCancellationSource_NoTokenInfo_ReturnsUnknown() {
        var ex = new OperationCanceledException("cancelled");
        Assert.Equal(CancellationSource.Unknown, AzureServiceBase<TestConfig>.GetCancellationSource(ex, CancellationToken.None));
    }

    private class TestConfig { public string Name { get; set; } = "test"; }

    private class TestService : AzureServiceBase<TestConfig> {
        public TestService(TestConfig config, IMemoryCache cache, string credentialId, IJsonLogger? logger = null)
            : base(config, cache, credentialId, logger) { }

        public static new void VerifyArgs(params string[] args) =>
            AzureServiceBase<TestConfig>.VerifyArgs(args);

        public new Task<T> LoggedOperationAsync<T>(string operationName, Func<Task<T>> operation, CancellationToken cancellationToken = default) =>
            base.LoggedOperationAsync(operationName, operation, cancellationToken);

        public new Task LoggedOperationAsync(string operationName, Func<Task> operation, CancellationToken cancellationToken = default) =>
            base.LoggedOperationAsync(operationName, operation, cancellationToken);

        public new Task<T> GetOrCreateCachedAsync<T>(string key, Func<Task<T>> factory, MemoryCacheEntryOptions? options = null) where T : notnull =>
            base.GetOrCreateCachedAsync(key, factory, options);
    }
}
