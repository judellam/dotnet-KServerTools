namespace KServerTools.Tests;

using KServerTools.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

public class KSTBuilderTests {
    [Fact]
    public void AddKServerTools_RegistersCommonServices() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKServerTools(kst => kst.AddCommon());

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ConfigurationHelper>());
        Assert.NotNull(provider.GetService<IDefaultCredential>());
    }

    [Fact]
    public void UseCredential_SetsDefaultAndRegistersCommon() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKServerTools(kst => kst
            .UseCredential<IDefaultCredential>()
        );

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ConfigurationHelper>());
    }

    [Fact]
    public void AddLogger_RegistersJsonLogger() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
        services.AddSingleton<IRequestContextAccessor>(new MockRequestContextAccessor());
        services.AddLogging();

        services.AddKServerTools(kst => kst
            .AddCommon()
            .AddLogger()
        );

        var provider = services.BuildServiceProvider();
        var logger = provider.GetService<IJsonLogger>();
        Assert.NotNull(logger);
        Assert.IsType<JsonLogger>(logger);
    }

    [Fact]
    public void AddILogger_RegistersILoggerAdapter() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        services.AddKServerTools(kst => kst
            .AddCommon()
            .AddILogger<KSTBuilderTests>()
        );

        var provider = services.BuildServiceProvider();
        var logger = provider.GetService<IJsonLogger>();
        Assert.NotNull(logger);
        Assert.IsType<ILoggerAdapter<KSTBuilderTests>>(logger);
    }

    [Fact]
    public void AddKeyVault_WithoutCredential_Throws() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        Assert.Throws<InvalidOperationException>(() =>
            services.AddKServerTools(kst => kst
                .AddCommon()
                .AddKeyVault<TestKeyVaultConfig>("TestSection")
            )
        );
    }

    [Fact]
    public void AddSecretResolver_Registers() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKServerTools(kst => kst
            .AddCommon()
            .AddSecretResolver()
        );

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ISecretResolver>());
    }

    [Fact]
    public void FluentChaining_AllMethodsReturnBuilder() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        services.AddSingleton<Microsoft.AspNetCore.Http.IHttpContextAccessor, Microsoft.AspNetCore.Http.HttpContextAccessor>();
        services.AddSingleton<IRequestContextAccessor>(new MockRequestContextAccessor());

        // Verify fluent API compiles and doesn't throw during registration
        services.AddKServerTools(kst => kst
            .UseCredential<IDefaultCredential>()
            .AddLogger()
            .AddSecretResolver()
        );

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<IJsonLogger>());
        Assert.NotNull(provider.GetService<ISecretResolver>());
    }

    private class TestKeyVaultConfig : IAzureKeyVaultConfiguration {
        public string Uri { get; set; } = "https://test.vault.azure.net/";
        public int CacheDurationInSeconds { get; set; } = 300;
    }

    private class MockRequestContextAccessor : IRequestContextAccessor {
        public IRequestContext? GetRequestContext() => null;
    }

    // --- Additional builder coverage ---

    [Fact]
    public void AddBlobStorage_WithoutCredential_Throws() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        Assert.Throws<InvalidOperationException>(() =>
            services.AddKServerTools(kst => kst
                .AddCommon()
                .AddBlobStorage<TestStorageConfig>("StorageSection")
            )
        );
    }

    [Fact]
    public void AddQueue_WithoutCredential_Throws() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        Assert.Throws<InvalidOperationException>(() =>
            services.AddKServerTools(kst => kst
                .AddCommon()
                .AddQueue<TestStorageConfig>("QueueSection")
            )
        );
    }

    [Fact]
    public void AddCosmosDb_WithoutCredential_Throws() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        Assert.Throws<InvalidOperationException>(() =>
            services.AddKServerTools(kst => kst
                .AddCommon()
                .AddCosmosDb<TestCosmosConfig>("CosmosSection")
            )
        );
    }

    [Fact]
    public void AddSql_WithoutCredential_Throws() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        Assert.Throws<InvalidOperationException>(() =>
            services.AddKServerTools(kst => kst
                .AddCommon()
                .AddSql<TestSqlConfig>()
            )
        );
    }

    [Fact]
    public void AddSqlConnectionString_DoesNotRequireCredential() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // Should not throw — connection string auth doesn't need a credential
        services.AddKServerTools(kst => kst
            .AddCommon()
            .AddSqlConnectionString<TestSqlConfig>()
        );

        Assert.Contains(services, sd => sd.ServiceType == typeof(ISqlServerService<TestSqlConfig>));
    }

    [Fact]
    public void AddCommon_IsIdempotent() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        services.AddKServerTools(kst => kst
            .AddCommon()
            .AddCommon()
            .AddCommon()
        );

        // ConfigurationHelper should be registered only once
        Assert.Single(services, sd => sd.ServiceType == typeof(ConfigurationHelper));
    }

    [Fact]
    public void EnsureCommon_AutoRegistersWhenNotExplicit() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());

        // AddSecretResolver calls EnsureCommon internally — should auto-register common
        services.AddKServerTools(kst => kst.AddSecretResolver());

        var provider = services.BuildServiceProvider();
        Assert.NotNull(provider.GetService<ConfigurationHelper>());
        Assert.NotNull(provider.GetService<ISecretResolver>());
    }

    [Fact]
    public void AddKeyVault_WithExplicitCredential_RegistersService() {
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["TestKv:Uri"] = "https://myvault.vault.azure.net/",
                ["TestKv:CacheDurationInSeconds"] = "300"
            }).Build());

        services.AddKServerTools(kst => kst
            .AddCommon()
            .AddKeyVault<TestKeyVaultConfig, IDefaultCredential>("TestKv")
        );

        Assert.Contains(services, sd => sd.ServiceType == typeof(IAzureKeyVaultService<TestKeyVaultConfig>));
    }

    private class TestStorageConfig : IAzureStorageServiceConfig {
        public string AccountName { get; set; } = "testaccount";
        public string Endpoint { get; set; } = "blob.core.windows.net";
    }

    private class TestCosmosConfig : IAzureCosmosDbConfiguration {
        public string EndpointUri { get; set; } = "https://test.documents.azure.com:443/";
        public string PrimaryKey { get; set; } = string.Empty;
    }

    private class TestSqlConfig : ISqlServerDatabaseConfiguration {
        public string Server { get; } = "localhost";
        public string Database { get; } = "testdb";
        public string[] Scopes { get; } = ["https://database.windows.net/.default"];
        public string? ConnectionStringData { get; } = "Server=localhost;Database=testdb;";
        public Task<string?> GetConnectionString(CancellationToken cancellationToken) =>
            Task.FromResult<string?>(this.ConnectionStringData);
    }
}
