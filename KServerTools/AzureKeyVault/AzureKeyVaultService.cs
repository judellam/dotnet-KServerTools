namespace KServerTools.Common;

using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Cryptography.X509Certificates;

/// <summary>
/// The Azure Key Vault Service
/// </summary>
internal class AzureKeyVaultService<T, C> : AzureServiceBase<T>, IAzureKeyVaultService<T> where T: class, IAzureKeyVaultConfiguration where C: ITokenCredentialService {
    private readonly C credentialResolver;
    private readonly Uri keyVaultUri;
    private const string SecretPrefix = "secret-";
    private const string CertificatePrefix = "certificate-";

    public AzureKeyVaultService(T azureKeyVaultConfiguration, C credentialResolver, IMemoryCache memoryCache, IJsonLogger logger)
        : base(azureKeyVaultConfiguration, memoryCache, typeof(C).FullName ?? typeof(C).Name, logger) {
        this.keyVaultUri = new Uri(azureKeyVaultConfiguration.Uri);
        this.credentialResolver = credentialResolver;
    }

    public Task<X509Certificate2> GetCertificate(string certificateName, CancellationToken cancellationToken) {
        string cacheKey = $"akv:{this.keyVaultUri.Host}:{CertificatePrefix}{certificateName}";
        var cacheOptions = new MemoryCacheEntryOptions {
            AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(this.config.CacheDurationInSeconds)
        };

        return this.LoggedOperationAsync($"Azure Key Vault certificate: {certificateName}", async () => {
            return await this.GetOrCreateCachedAsync(cacheKey, async () => {
                TokenCredential credential = await this.credentialResolver.GetCredential(cancellationToken)
                    .ConfigureAwait(false);

                CertificateClient certificateClient = new(this.keyVaultUri, credential);
                cancellationToken.ThrowIfCancellationRequested();
                KeyVaultCertificateWithPolicy keyVaultCertificate = await certificateClient.GetCertificateAsync(certificateName, cancellationToken)
                    .ConfigureAwait(false);

                return X509CertificateLoader.LoadCertificate(keyVaultCertificate.Cer);
            }, cacheOptions).ConfigureAwait(false);
        });
    }

    public Task<string> GetSecretAsync(string secretName, CancellationToken cancellationToken) {
        string cacheKey = $"akv:{this.keyVaultUri.Host}:{SecretPrefix}{secretName}";
        var cacheOptions = new MemoryCacheEntryOptions {
            AbsoluteExpiration = DateTimeOffset.Now.AddSeconds(this.config.CacheDurationInSeconds)
        };

        return this.LoggedOperationAsync($"Azure Key Vault secret: {secretName}", async () => {
            return await this.GetOrCreateCachedAsync(cacheKey, async () => {
                TokenCredential credential = await this.credentialResolver.GetCredential(cancellationToken)
                    .ConfigureAwait(false);

                SecretClient secretClient = new(this.keyVaultUri, credential);
                KeyVaultSecret secret = await secretClient.GetSecretAsync(secretName, null, cancellationToken)
                    .ConfigureAwait(false);

                return secret.Value;
            }, cacheOptions).ConfigureAwait(false);
        });
    }
}