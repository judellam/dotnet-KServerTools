namespace KServerTools.Common;

using System.Threading.Tasks;
using Azure.Core;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;

internal class AzureStorageServiceInternal<T, C>(T config, C credential) : AzureStorageBase<T,C>(config, credential), IAzureStorageService<T> where T : IAzureStorageServiceConfig where C: ITokenCredentialService {
    public async Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        
        await blobClient.UploadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient blobContainerClient = await this.GetContainerClient(containerName, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = blobContainerClient.GetBlobClient(blobName);

        Stream stream = new MemoryStream();
        await blobClient.DownloadToAsync(stream, cancellationToken)
            .ConfigureAwait(false);

        stream.Position = 0;
        return stream;
    }

    public async Task AppendAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Verify(containerName, blobName);
        BlobContainerClient blobContainerClient = await this.GetContainerClient(containerName, cancellationToken)
            .ConfigureAwait(false);

        AppendBlobClient blobClient = blobContainerClient.GetAppendBlobClient(blobName);
        await blobClient.CreateIfNotExistsAsync()
            .ConfigureAwait(false);
        
        await blobClient.AppendBlockAsync(stream, null, cancellationToken)
            .ConfigureAwait(false);
    }

    protected async Task<BlobContainerClient> GetContainerClient(string containerName, CancellationToken cancellationToken) {
        BlobContainerClient client;
        string key = $"{this.config.AccountName}:{containerName}";
        if (!this.memoryCache.TryGetValue(key, out client!)) {
            Uri storageUri = new($"https://{config.AccountName}.{config.Endpoint}/{containerName}");
            client = new(storageUri, await this.credential.GetCredential(cancellationToken));

            await client.CreateIfNotExistsAsync()
                .ConfigureAwait(false);

            this.memoryCache.Set(key, client, memoryCacheEntryOptions);
        }
        return client;
    }
}