namespace KServerTools.Common;

using Azure.Core;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;

/// <summary>
/// The Azure Key Value Service
/// </summary>
public interface IAzureKeyVaultService {
    Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken);
}

/// <summary>
/// The Azure Key Vault Service
/// </summary>
/// <remarks>
/// This service is responsible for retrieving secrets from Azure Key Vault.
/// Requires: dotnet add package Microsoft.Extensions.Caching.Memory
/// Requires: dotnet add package Azure.Security.KeyVault.Secrets
/// </remarks>
internal class AzureKeyVaultService<T> : IAzureKeyVaultService where T: ICredentialResolver {
    private readonly IAzureKeyVaultConfiguration azureKeyVaultConfiguration;
    private readonly T credentialResolver;
    private readonly IMemoryCache memoryCache;
    private readonly IJsonLogger logger;
    private readonly Uri keyVaultUri;

    public AzureKeyVaultService(IAzureKeyVaultConfiguration azureKeyVaultConfiguration, T credentialResolver, IMemoryCache memoryCache, IJsonLogger logger) {
        this.azureKeyVaultConfiguration = azureKeyVaultConfiguration;
        this.keyVaultUri = new Uri(this.azureKeyVaultConfiguration.Uri);

        this.credentialResolver = credentialResolver;
        this.memoryCache = memoryCache;
        this.logger = logger;
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            if (this.memoryCache.TryGetValue(secretName, out string? secretValue) && !string.IsNullOrWhiteSpace(secretValue)) {
                return secretValue;
            }

            TokenCredential credential = await this.credentialResolver.GetCredential(cancellationToken)
                .ConfigureAwait(false);
            
            SecretClient secretClient = new(this.keyVaultUri, credential);
            KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName, null, cancellationToken)
                .ConfigureAwait(false);
            
            secretValue = secret.Value;
            this.memoryCache.Set(
                secretName,
                secretValue,
                DateTimeOffset.Now.AddSeconds(this.azureKeyVaultConfiguration.CacheDurationInSeconds));

            return secretValue;
        } catch (Exception ex) {
            this.logger.Error($"Failed to retrieve secret from Azure Key Vault: {secretName}", ex);
            throw;
        } finally {
            stopwatch.Stop();
            this.logger.Info($"Azure Key Vault request: {secretName}, Elapsed: {stopwatch.ElapsedMilliseconds}ms");
        }
    }
}