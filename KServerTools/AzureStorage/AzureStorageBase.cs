namespace KServerTools.Common;

using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Abstract base class for Azure Storage services providing shared configuration and credential management.
/// </summary>
/// <typeparam name="T">The configuration type implementing <see cref="IAzureStorageServiceConfig"/>.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
/// <param name="config">The storage service configuration.</param>
/// <param name="credential">The token credential used for authentication.</param>
/// <param name="memoryCache">The memory cache for client reuse.</param>
/// <param name="logger">An optional JSON logger instance.</param>
internal class AzureStorageBase<T, C>(T config, C credential, IMemoryCache memoryCache, IJsonLogger? logger = null)
    : AzureServiceBase<T>(config, memoryCache, typeof(C).FullName ?? typeof(C).Name, logger) where T : class, IAzureStorageServiceConfig where C : ITokenCredentialService {
    /// <summary>
    /// The token credential used for authenticating with Azure Storage.
    /// </summary>
    protected readonly C credential = credential;

    /// <summary>
    /// Gets the storage service configuration.
    /// </summary>
    public T Config {
        get {
            return this.config;
        }
    }

    /// <summary>
    /// Validates that the specified arguments are not null or empty.
    /// </summary>
    /// <param name="args">The arguments to validate.</param>
    protected static void Verify(params string[] args) => VerifyArgs(args);
}
