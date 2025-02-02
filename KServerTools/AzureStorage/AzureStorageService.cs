namespace KServerTools.Common;

using System.Diagnostics;
using System.Threading.Tasks;

// https://dastr.dfs.core.windows.net/
// DefaultEndpointsProtocol=https;AccountName=dastr;EndpointSuffix=core.windows.net

internal class AzureStorageService<T, C>(T config, C credential, IJsonLogger logger) : IAzureStorageService where T: IAzureStorageServiceConfig where C: ITokenCredentialService{
    private readonly IAzureStorageService service = new AzureStorageServiceInternal<T, C>(config, credential);
    private readonly IJsonLogger logger = logger;

    public async Task UploadBlobAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            await this.service.UploadBlobAsync(containerName, blobName, stream, cancellationToken)
                .ConfigureAwait(false);
        } catch (Exception ex) {
            this.logger.Error($"Failed to upload blob {blobName} to container {containerName}", ex);
            throw;
        }
        finally {
            stopwatch.Stop();
            this.logger.Info($"Uploaded blob {blobName} to container {containerName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<Stream> DownloadBlobAsync(string containerName, string blobName, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            return await this.service.DownloadBlobAsync(containerName, blobName, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception ex) {
            this.logger.Error($"Failed to download blob {blobName} from container {containerName}", ex);
            throw;
        }
        finally {
            stopwatch.Stop();
            this.logger.Info($"Downloaded blob {blobName} from container {containerName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public Task AppendAsync(string containerName, string blobName, Stream stream, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            return this.service.AppendAsync(containerName, blobName, stream, cancellationToken);
        }
        catch (Exception ex) {
            this.logger.Error($"Failed to append to blob {blobName} in container {containerName}", ex);
            throw;
        }
        finally {
            stopwatch.Stop();
            this.logger.Info($"Appended to blob {blobName} in container {containerName}", stopwatch.ElapsedMilliseconds);
        }
    }
}