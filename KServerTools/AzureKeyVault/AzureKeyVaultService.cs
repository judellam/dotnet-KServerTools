namespace KServerTools.Common;

using System.Security.Cryptography.X509Certificates;
using Azure.Core;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Internal Azure Key Vault service implementation.
/// </summary>
/// <typeparam name="T">The configuration type implementing <see cref="IAzureKeyVaultConfiguration"/>.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
internal class AzureKeyVaultService<T, C> : AzureServiceBase<T>, IAzureKeyVaultService<T> where T : class, IAzureKeyVaultConfiguration where C : ITokenCredentialService {
    private const string SecretPrefix = "secret-";
    private const string CertificatePrefix = "certificate-";
    private readonly C credentialResolver;
    private readonly Uri keyVaultUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureKeyVaultService{T, C}"/> class.
    /// </summary>
    /// <param name="azureKeyVaultConfiguration">The Key Vault configuration.</param>
    /// <param name="credentialResolver">The credential resolver for authentication.</param>
    /// <param name="memoryCache">The memory cache for secret and certificate caching.</param>
    /// <param name="logger">The JSON logger instance.</param>
    public AzureKeyVaultService(T azureKeyVaultConfiguration, C credentialResolver, IMemoryCache memoryCache, IJsonLogger logger)
        : base(azureKeyVaultConfiguration, memoryCache, typeof(C).FullName ?? typeof(C).Name, logger) {
        this.keyVaultUri = new Uri(azureKeyVaultConfiguration.Uri);
        this.credentialResolver = credentialResolver;
    }

    /// <summary>
    /// Retrieves a certificate from Azure Key Vault.
    /// </summary>
    /// <param name="certificateName">The name of the certificate to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The retrieved <see cref="X509Certificate2"/>.</returns>
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

    /// <summary>
    /// Retrieves a secret value from Azure Key Vault.
    /// </summary>
    /// <param name="secretName">The name of the secret to retrieve.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The secret value as a string.</returns>
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
