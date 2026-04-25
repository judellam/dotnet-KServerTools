namespace KServerTools.Tests;

using KServerTools.Common;
using Moq;

public class SecretResolverTests {
    [Fact]
    public async Task Resolve_LocalSecret_ReturnsAsIs() {
        var resolver = new SecretResolver();
        string result = await resolver.Resolve("my-plain-secret", CancellationToken.None);
        Assert.Equal("my-plain-secret", result);
    }

    [Fact]
    public async Task Resolve_AkvScheme_WithoutRegistration_Throws() {
        var resolver = new SecretResolver();
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => resolver.Resolve("akv://mySecretName", CancellationToken.None).AsTask());
    }

    [Fact]
    public void RegisterKeyVaultService_SecondCall_Throws() {
        var resolver = new SecretResolver();
        var mockService = new Moq.Mock<IAzureKeyVaultInternal>();

        resolver.RegisterKeyVaultService(mockService.Object);

        var ex = Assert.Throws<InvalidOperationException>(
            () => resolver.RegisterKeyVaultService(mockService.Object));
        Assert.Contains("immutable", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Resolve_AkvScheme_WithRegistration_CallsKeyVault() {
        var resolver = new SecretResolver();
        var mockService = new Moq.Mock<IAzureKeyVaultInternal>();
        // Uri.Host lowercases the value, so mock must match lowercase
        mockService.Setup(s => s.GetSecretAsync("mysecretname", It.IsAny<CancellationToken>()))
            .ReturnsAsync("resolved-value");

        resolver.RegisterKeyVaultService(mockService.Object);

        string result = await resolver.Resolve("akv://mySecretName", CancellationToken.None);
        Assert.Equal("resolved-value", result);
        mockService.Verify(s => s.GetSecretAsync("mysecretname", It.IsAny<CancellationToken>()), Moq.Times.Once);
    }

    [Fact]
    public async Task Resolve_EmptyString_ReturnsAsIs() {
        var resolver = new SecretResolver();
        string result = await resolver.Resolve("", CancellationToken.None);
        Assert.Equal("", result);
    }

    [Fact]
    public async Task Resolve_HttpsUri_TreatedAsLocalSecret() {
        var resolver = new SecretResolver();
        string result = await resolver.Resolve("https://example.com/path", CancellationToken.None);
        Assert.Equal("https://example.com/path", result);
    }

    [Fact]
    public async Task Resolve_ConnectionString_TreatedAsLocalSecret() {
        var resolver = new SecretResolver();
        string connStr = "Server=myserver;Database=mydb;User=admin;Password=secret123";
        string result = await resolver.Resolve(connStr, CancellationToken.None);
        Assert.Equal(connStr, result);
    }
}
