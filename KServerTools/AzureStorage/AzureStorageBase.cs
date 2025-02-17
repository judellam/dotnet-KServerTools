namespace KServerTools.Common;

using Microsoft.Extensions.Caching.Memory;

internal class AzureStorageBase<T,C>(T config, C credential) where T : IAzureStorageServiceConfig where C : ITokenCredentialService {
    protected readonly T config = config;    
    protected readonly C credential = credential;
    protected readonly MemoryCache memoryCache = new(new MemoryCacheOptions());
    protected static readonly MemoryCacheEntryOptions memoryCacheEntryOptions = new() {
        SlidingExpiration = TimeSpan.FromMinutes(50)
    };

    public T Config {
        get {
            return this.config;
        }
    }

    protected static void Verify(params string[] args) {
        foreach (string arg in args) {
            if (string.IsNullOrWhiteSpace(arg)) {
                throw new ArgumentException("Argument cannot be null or empty.");
            }
        }
    }
}