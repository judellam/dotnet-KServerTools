namespace KServerTools.Common;

using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;

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
    private const string SecretPrefix = "secret-";
    private const string CertificatePrefix = "certificate-";

    public AzureKeyVaultService(IAzureKeyVaultConfiguration azureKeyVaultConfiguration, T credentialResolver, IMemoryCache memoryCache, IJsonLogger logger) {
        this.azureKeyVaultConfiguration = azureKeyVaultConfiguration;
        this.keyVaultUri = new Uri(this.azureKeyVaultConfiguration.Uri);

        this.credentialResolver = credentialResolver;
        this.memoryCache = memoryCache;
        this.logger = logger;
    }

    public async Task<X509Certificate2> GetCertificate(string certificateName, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string cacheLookup = $"{CertificatePrefix}{certificateName}";
        try {
            this.logger.Info($"Retrieving certificate from Azure Key Vault: {this.keyVaultUri}, Cerificate: {certificateName}");

            X509Certificate2? certificate;
            if (this.memoryCache.TryGetValue(cacheLookup, out certificate) && certificate != null) {
                return certificate;
            }

            TokenCredential credential = await this.credentialResolver.GetCredential(cancellationToken)
                .ConfigureAwait(false);

            CertificateClient certificateClient = new(this.keyVaultUri, credential);
            cancellationToken.ThrowIfCancellationRequested();
            KeyVaultCertificateWithPolicy keyVaultCertificate = await certificateClient.GetCertificateAsync(certificateName, cancellationToken)
                .ConfigureAwait(false);

            certificate = X509CertificateLoader.LoadCertificate(keyVaultCertificate.Cer);

            this.memoryCache.Set(
                cacheLookup,
                certificate,
                DateTimeOffset.Now.AddSeconds(this.azureKeyVaultConfiguration.CacheDurationInSeconds));

            return certificate;
        } catch (Exception ex) {
            this.logger.Error($"Failed to retrieve certificate from Azure Key Vault: {certificateName}", ex);
            throw;
        } finally {
            stopwatch.Stop();
            this.logger.Info($"Azure Key Vault request: {certificateName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        string cacheLookup = $"{SecretPrefix}{secretName}";
        try {
            if (this.memoryCache.TryGetValue(cacheLookup, out string? secretValue) && !string.IsNullOrWhiteSpace(secretValue)) {
                return secretValue;
            }

            TokenCredential credential = await this.credentialResolver.GetCredential(cancellationToken)
                .ConfigureAwait(false);
            
            SecretClient secretClient = new(this.keyVaultUri, credential);
            KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName, null, cancellationToken)
                .ConfigureAwait(false);
            
            secretValue = secret.Value;
            this.memoryCache.Set(
                cacheLookup,
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