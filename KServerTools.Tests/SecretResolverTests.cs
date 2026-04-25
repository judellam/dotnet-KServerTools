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
}
