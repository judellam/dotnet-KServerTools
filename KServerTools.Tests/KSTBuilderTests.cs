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
}
