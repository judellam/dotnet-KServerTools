namespace KServerTools.Common;

using Microsoft.Extensions.Caching.Memory;

internal class AzureStorageBase<T,C>(T config, C credential, IMemoryCache memoryCache, IJsonLogger? logger = null) 
    : AzureServiceBase<T>(config, memoryCache, typeof(C).FullName ?? typeof(C).Name, logger) where T : class, IAzureStorageServiceConfig where C : ITokenCredentialService {
    protected readonly C credential = credential;

    public T Config {
        get {
            return this.config;
        }
    }

    protected static void Verify(params string[] args) => VerifyArgs(args);
}