namespace KServerTools.Tests;

using KServerTools.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Tests for DependencyHelper registration methods and AddConfigSection behavior.
/// </summary>
public class DependencyHelperTests {
    private static IServiceCollection CreateServicesWithConfig(Dictionary<string, string?>? configValues = null) {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(configValues ?? new Dictionary<string, string?>())
            .Build();
        services.AddSingleton<IConfiguration>(config);
        return services;
    }

    // --- KSTAddCommon ---

    [Fact]
    public void KSTAddCommon_RegistersConfigurationHelper() {
        var services = CreateServicesWithConfig();
        services.KSTAddCommon();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ConfigurationHelper>());
    }

    [Fact]
    public void KSTAddCommon_RegistersDefaultCredential() {
        var services = CreateServicesWithConfig();
        services.KSTAddCommon();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IDefaultCredential>());
    }

    [Fact]
    public void KSTAddCommon_RegistersMemoryCache() {
        var services = CreateServicesWithConfig();
        services.KSTAddCommon();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<Microsoft.Extensions.Caching.Memory.IMemoryCache>());
    }

    // --- KSTAddLogger ---

    [Fact]
    public void KSTAddLogger_RegistersJsonLogger() {
        var services = CreateServicesWithConfig();
        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
        services.AddSingleton<IRequestContextAccessor>(new TestRequestContextAccessor());
        services.AddLogging();
        services.KSTAddLogger();
        var provider = services.BuildServiceProvider();

        var logger = provider.GetService<IJsonLogger>();
        Assert.NotNull(logger);
        Assert.IsType<JsonLogger>(logger);
    }

    [Fact]
    public void KSTAddLogger_Generic_RegistersILoggerAdapter() {
        var services = CreateServicesWithConfig();
        services.AddLogging();
        services.KSTAddLogger<DependencyHelperTests>();
        var provider = services.BuildServiceProvider();

        var logger = provider.GetService<IJsonLogger>();
        Assert.NotNull(logger);
        Assert.IsType<ILoggerAdapter<DependencyHelperTests>>(logger);
    }

    // --- KSTAddSecretResolver ---

    [Fact]
    public void KSTAddSecretResolver_RegistersISecretResolver() {
        var services = CreateServicesWithConfig();
        services.KSTAddSecretResolver();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<ISecretResolver>());
    }

    // --- KSTAddRequestContext ---

    [Fact]
    public void KSTAddRequestContext_RegistersAccessors() {
        var services = CreateServicesWithConfig();
        services.KSTAddRequestContext<TestRequestContext>();
        var provider = services.BuildServiceProvider();

        Assert.NotNull(provider.GetService<IRequestContextAccessor>());
        Assert.NotNull(provider.GetService<Microsoft.AspNetCore.Http.IHttpContextAccessor>());
    }

    // --- AddConfigSection ---

    [Fact]
    public void AddConfigSection_ValidSection_ResolvesConfig() {
        var services = CreateServicesWithConfig(new Dictionary<string, string?> {
            ["TestKvConfig:Uri"] = "https://myvault.vault.azure.net/",
            ["TestKvConfig:CacheDurationInSeconds"] = "300"
        });
        services.KSTAddCommon();
        services.AddConfigSection<TestKvConfig>("TestKvConfig");
        var provider = services.BuildServiceProvider();

        var config = provider.GetService<TestKvConfig>();
        Assert.NotNull(config);
        Assert.Equal("https://myvault.vault.azure.net/", config!.Uri);
    }

    [Fact]
    public void AddConfigSection_MissingSection_ThrowsInvalidOperationException() {
        var services = CreateServicesWithConfig();
        services.KSTAddCommon();
        services.AddConfigSection<TestKvConfig>("NonExistent");
        var provider = services.BuildServiceProvider();

        var ex = Assert.Throws<InvalidOperationException>(() => provider.GetService<TestKvConfig>());
        Assert.Contains("TestKvConfig", ex.Message);
        Assert.Contains("NonExistent", ex.Message);
    }

    // --- KSTAddSqlServiceConnectionString ---

    [Fact]
    public void KSTAddSqlServiceConnectionString_RegistersService() {
        var services = CreateServicesWithConfig();
        services.KSTAddSqlServiceConnectionString<TestSqlConfig>();

        // Verify service descriptor was registered (can't resolve without real config)
        Assert.Contains(services, sd => sd.ServiceType == typeof(ISqlServerService<TestSqlConfig>));
    }

    // --- AddKServerTools entry point ---

    [Fact]
    public void AddKServerTools_InvokesBuilderAction() {
        var services = CreateServicesWithConfig();
        bool actionCalled = false;

        services.AddKServerTools(kst => { actionCalled = true;
            kst.AddCommon(); });

        Assert.True(actionCalled);
    }

    // --- Test helper types ---

    private class TestKvConfig : IAzureKeyVaultConfiguration {
        public string Uri { get; set; } = string.Empty;
        public int CacheDurationInSeconds { get; set; }
    }

    private class TestSqlConfig : ISqlServerDatabaseConfiguration {
        public string Server { get; } = "localhost";
        public string Database { get; } = "testdb";
        public string[] Scopes { get; } = ["https://database.windows.net/.default"];
        public string? ConnectionStringData { get; } = "Server=localhost;Database=testdb;";
        public Task<string?> GetConnectionString(CancellationToken cancellationToken) =>
            Task.FromResult<string?>(this.ConnectionStringData);
    }

    private class TestRequestContext : IRequestContext {
        public Guid RequestId { get; } = Guid.NewGuid();
        public string? UserAgent { get; } = "test-agent";
        public void Setup(Microsoft.AspNetCore.Http.HttpContext context) { }
    }

    private class TestRequestContextAccessor : IRequestContextAccessor {
        public IRequestContext? GetRequestContext() => null;
    }
}
