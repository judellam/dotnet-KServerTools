namespace KServerTools.Tests;

using KServerTools.Common;
using Microsoft.Extensions.Configuration;

public class ConfigurationHelperTests {
    [Fact]
    public void TryGet_WithValidSection_ReturnsConfig() {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["TestConfig:Uri"] = "https://test.vault.azure.net/",
                ["TestConfig:CacheDurationInSeconds"] = "600",
            })
            .Build();

        var helper = new ConfigurationHelper(config);
        var result = helper.TryGet<TestAkvConfig>("TestConfig");

        Assert.NotNull(result);
        Assert.Equal("https://test.vault.azure.net/", result!.Uri);
        Assert.Equal(600, result.CacheDurationInSeconds);
    }

    [Fact]
    public void TryGet_WithMissingSection_ReturnsNull() {
        var config = new ConfigurationBuilder().Build();
        var helper = new ConfigurationHelper(config);

        var result = helper.TryGet<TestAkvConfig>("NonExistent");
        Assert.Null(result);
    }

    [Fact]
    public void TryGet_WithoutSectionName_UsesTypeName() {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["TestAkvConfig:Uri"] = "https://auto.vault.azure.net/",
                ["TestAkvConfig:CacheDurationInSeconds"] = "100",
            })
            .Build();

        var helper = new ConfigurationHelper(config);
        var result = helper.TryGet<TestAkvConfig>();

        Assert.NotNull(result);
        Assert.Equal("https://auto.vault.azure.net/", result!.Uri);
    }

    [Fact]
    public void Constructor_ThrowsOnNullConfiguration() {
        Assert.Throws<ArgumentNullException>(() => new ConfigurationHelper(null!));
    }

    private class TestAkvConfig {
        public string Uri { get; set; } = "";
        public int CacheDurationInSeconds { get; set; }
    }
}
