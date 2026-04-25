namespace KServerTools.Common;

using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Caching.Memory;

internal class AzureStorageServiceInternal<T, C>(T config, C credential, IMemoryCache memoryCache) : AzureStorageBase<T, C>(config, credential, memoryCache), IAzureStorageService<T>, IAzureBlobManagementService<T> where T : class, IAzureStorageServiceConfig where C : ITokenCredentialService {
    public async Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, true, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);

        await blobClient.UploadAsync(stream, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient blobContainerClient = await this.GetContainerClient(containerName, false, cancellationToken)
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
        BlobContainerClient blobContainerClient = await this.GetContainerClient(containerName, true, cancellationToken)
            .ConfigureAwait(false);

        AppendBlobClient blobClient = blobContainerClient.GetAppendBlobClient(blobName);
        await blobClient.CreateIfNotExistsAsync()
            .ConfigureAwait(false);

        await blobClient.AppendBlockAsync(stream, null, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<bool> DeleteBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.DeleteIfExistsAsync(Azure.Storage.Blobs.Models.DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken)
            .ConfigureAwait(false);
        return response.Value;
    }

    public async Task<bool> BlobExistsAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Verify(containerName, blobName);

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        BlobClient blobClient = containerClient.GetBlobClient(blobName);
        var response = await blobClient.ExistsAsync(cancellationToken).ConfigureAwait(false);
        return response.Value;
    }

    public async IAsyncEnumerable<string> ListBlobsAsync(string containerName, string? prefix, [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(containerName, nameof(containerName));

        BlobContainerClient containerClient = await this.GetContainerClient(containerName, false, cancellationToken)
            .ConfigureAwait(false);

        await foreach (var blobItem in containerClient.GetBlobsAsync(prefix: prefix, cancellationToken: cancellationToken).ConfigureAwait(false)) {
            yield return blobItem.Name;
        }
    }

    public async Task<IReadOnlyList<string>> ListBlobsToListAsync(string containerName, string? prefix, CancellationToken cancellationToken) {
        var results = new List<string>();
        await foreach (var name in this.ListBlobsAsync(containerName, prefix, cancellationToken).ConfigureAwait(false)) {
            results.Add(name);
        }

        return results;
    }

    protected async Task<BlobContainerClient> GetContainerClient(string containerName, bool createIfNotExists, CancellationToken cancellationToken) {
        string key = $"blob:{this.config.AccountName}:{containerName}";
        return await this.GetOrCreateCachedAsync(key, async () => {
            Uri storageUri = new($"https://{this.config.AccountName}.{this.config.Endpoint}/{containerName}");
            var client = new BlobContainerClient(storageUri, await this.credential.GetCredential(cancellationToken));

            if (createIfNotExists) {
                await client.CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return client;
        }).ConfigureAwait(false);
    }
}
