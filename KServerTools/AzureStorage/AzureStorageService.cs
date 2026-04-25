namespace KServerTools.Common;

using System.Threading.Tasks;

internal class AzureStorageService<T, C>(T config, C credential, IJsonLogger logger, Microsoft.Extensions.Caching.Memory.IMemoryCache memoryCache) : IAzureStorageService<T> where T: class, IAzureStorageServiceConfig where C: ITokenCredentialService{
    private readonly IAzureStorageService<T> service = new AzureStorageServiceInternal<T, C>(config, credential, memoryCache);
    private readonly IJsonLogger logger = logger;

    public Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Uploaded blob {blobName} to container {containerName}",
            () => this.service.UploadBlobAsync(containerName, blobName, stream, cancellationToken));

    public Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Downloaded blob {blobName} from container {containerName}",
            () => this.service.DownloadBlobAsync(containerName, blobName, cancellationToken));

    public Task AppendAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) =>
        AzureServiceBaseHelpers.LoggedOperationAsync(this.logger,
            $"Appended to blob {blobName} in container {containerName}",
            () => this.service.AppendAsync(containerName, blobName, stream, cancellationToken));
}