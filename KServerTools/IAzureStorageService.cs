namespace KServerTools.Common;

public interface IAzureStorageService {
    public Task UploadBlobAsync(
        string containerName,
        string blobName,
        Stream stream,
        CancellationToken cancellationToken);

    public Task AppendAsync(
        string containerName,
        string blobName,
        Stream stream,
        CancellationToken cancellationToken);
    
    public Task<Stream> DownloadBlobAsync(
        string containerName,
        string blobName,
        CancellationToken cancellationToken);
}