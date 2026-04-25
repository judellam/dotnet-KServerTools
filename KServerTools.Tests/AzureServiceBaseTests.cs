namespace KServerTools.Tests;

using KServerTools.Common;
using Microsoft.Extensions.Caching.Memory;
using Moq;

public class AzureServiceBaseTests {
    private class TestConfig { public string Name { get; set; } = "test"; }

    private class TestService : AzureServiceBase<TestConfig> {
        public TestService(TestConfig config, IMemoryCache cache, string credentialId, IJsonLogger? logger = null)
            : base(config, cache, credentialId, logger) { }

        public new Task<T> LoggedOperationAsync<T>(string operationName, Func<Task<T>> operation) =>
            base.LoggedOperationAsync(operationName, operation);

        public new Task LoggedOperationAsync(string operationName, Func<Task> operation) =>
            base.LoggedOperationAsync(operationName, operation);

        public new Task<T> GetOrCreateCachedAsync<T>(string key, Func<Task<T>> factory, MemoryCacheEntryOptions? options = null) where T : notnull =>
            base.GetOrCreateCachedAsync(key, factory, options);
    }

    [Fact]
    public async Task LoggedOperationAsync_LogsSuccessWithLatency() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        var result = await service.LoggedOperationAsync("test-op", () => Task.FromResult(42));

        Assert.Equal(42, result);
        logger.Verify(l => l.Info("test-op", It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_LogsErrorAndRethrows() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.LoggedOperationAsync<int>("fail-op", () => throw new InvalidOperationException("boom")));

        logger.Verify(l => l.Error(
            It.Is<string>(s => s.Contains("fail-op")),
            It.IsAny<Exception>(),
            It.IsAny<long?>(),
            It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task LoggedOperationAsync_Void_LogsSuccess() {
        var logger = new Mock<IJsonLogger>();
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger.Object);

        bool executed = false;
        await service.LoggedOperationAsync("void-op", () => { executed = true; return Task.CompletedTask; });

        Assert.True(executed);
        logger.Verify(l => l.Info("void-op", It.IsAny<long?>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task GetOrCreateCachedAsync_CachesResult() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1");

        int factoryCalls = 0;
        var first = await service.GetOrCreateCachedAsync("key1", () => { factoryCalls++; return Task.FromResult("value1"); });
        var second = await service.GetOrCreateCachedAsync("key1", () => { factoryCalls++; return Task.FromResult("value2"); });

        Assert.Equal("value1", first);
        Assert.Equal("value1", second);
        Assert.Equal(1, factoryCalls);
    }

    [Fact]
    public async Task GetOrCreateCachedAsync_IsolatesByCredentialId() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service1 = new TestService(new TestConfig(), cache, "tenant-a");
        var service2 = new TestService(new TestConfig(), cache, "tenant-b");

        var result1 = await service1.GetOrCreateCachedAsync("shared-key", () => Task.FromResult("from-tenant-a"));
        var result2 = await service2.GetOrCreateCachedAsync("shared-key", () => Task.FromResult("from-tenant-b"));

        Assert.Equal("from-tenant-a", result1);
        Assert.Equal("from-tenant-b", result2);
    }

    [Fact]
    public async Task LoggedOperationAsync_WorksWithNullLogger() {
        using var cache = new MemoryCache(new MemoryCacheOptions());
        var service = new TestService(new TestConfig(), cache, "cred1", logger: null);

        var result = await service.LoggedOperationAsync("no-logger", () => Task.FromResult(99));
        Assert.Equal(99, result);
    }
}
