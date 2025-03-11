namespace KServerTools.Common;

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;

internal class AzureCosmosDb<T, C>(T configuration, C credential) : IAzureCosmosDb<T>, IDisposable where T : IAzureCosmosDbConfiguration where C : ITokenCredentialService {
    private readonly T configuration = configuration;
    private readonly C credential = credential;
    protected readonly MemoryCache memoryCache = new(new MemoryCacheOptions());
    protected static readonly MemoryCacheEntryOptions memoryCacheEntryOptions = new() {
        SlidingExpiration = TimeSpan.FromMinutes(50)
    };

    public async Task<bool> CreateDatabaseAsync(string database, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        CosmosClient client = await this.GetClient(database);
        DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(database, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<bool> CreateContainerAsync(string database, string container, string partitionKey, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(partitionKey, nameof(partitionKey));
        CosmosClient client = await this.GetClient(database);
        Database cosmosDatabase = client.GetDatabase(database);
        ContainerResponse response = await cosmosDatabase.CreateContainerIfNotExistsAsync(container, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<I> GetItemAsync<I>(string database, string container, string itemId, string partitionKey, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(partitionKey, nameof(partitionKey));
        Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
        ItemResponse<I> response = await cosmosContainer.ReadItemAsync<I>(itemId, new PartitionKey(partitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Resource;
    }

    public Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, string query, CancellationToken cancellationToken) where I : ICosmosEntity
    {
        throw new NotImplementedException();
    }

    public async Task<I> AddItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));

        Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
        ItemResponse<I> response = await cosmosContainer.CreateItemAsync<I>(item, new PartitionKey(item.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Resource;
    }

    public async Task<I> UpdateItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
        ItemResponse<I> response = await cosmosContainer.UpsertItemAsync<I>(item, new PartitionKey(item.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.Resource;
    }

    public async Task DeleteItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
        ItemResponse<I> response = await cosmosContainer.DeleteItemAsync<I>(item.Id, new PartitionKey(item.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public void Dispose() {
        this.memoryCache?.Dispose();
    }

    private async ValueTask<CosmosClient> GetClient(string databaseName) {
        CosmosClient client;
        string key = $"{databaseName}";
        if (!this.memoryCache.TryGetValue(key, out client!)) {
            var options = new CosmosClientOptions {
                SerializerOptions = new CosmosSerializationOptions {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            };
            if (this.credential != null) {
                client = new(this.configuration.EndpointUri, await this.credential.GetCredential(CancellationToken.None), options);
            } else {
                client = new(this.configuration.EndpointUri, this.configuration.PrimaryKey, options);
            }
            this.memoryCache.Set(key, client, memoryCacheEntryOptions);
        }

        return client;
    }

    private async Task<Container> GetContainer(string database, string container, CancellationToken cancellationToken) {
        CosmosClient client = await this.GetClient(database);
        Database cosmosDatabase = client.GetDatabase(database);
        return cosmosDatabase.GetContainer(container);
    }
}