namespace KServerTools.Common;

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;

internal class AzureCosmosDb<T, C>(T configuration, C credential, IMemoryCache memoryCache, IJsonLogger logger) : AzureServiceBase<T>(configuration, memoryCache, typeof(C).FullName ?? typeof(C).Name, logger), IAzureCosmosDb<T>, IDisposable where T : class, IAzureCosmosDbConfiguration where C : ITokenCredentialService {
    private readonly C credential = credential;

    public async Task<bool> CreateDatabaseAsync(string database, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        CosmosClient client = await this.GetClient();
        DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(database, cancellationToken: cancellationToken).ConfigureAwait(false);
        return response.StatusCode == HttpStatusCode.Created;
    }

    public async Task<bool> CreateContainerAsync(string database, string container, string partitionKey, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(partitionKey, nameof(partitionKey));
        CosmosClient client = await this.GetClient();
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

    public async Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, string query, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(query, nameof(query));

        return await GetItemsAsync<I>(database, container, new QueryDefinition(query), cancellationToken).ConfigureAwait(false);
    }

    public async Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, QueryDefinition queryDefinition, CancellationToken cancellationToken, QueryRequestOptions? requestOptions = null) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNull(queryDefinition, nameof(queryDefinition));

        Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
        using var iterator = cosmosContainer.GetItemQueryIterator<I>(queryDefinition, requestOptions: requestOptions);
        var results = new List<I>();
        while (iterator.HasMoreResults) {
            FeedResponse<I> response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
            results.AddRange(response);
        }
        return results;
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
        if (this.cosmosClient != null) {
            this.cosmosClient.Dispose();
            this.cosmosClient = null;
        }
    }

    private CosmosClient? cosmosClient;

    private async ValueTask<CosmosClient> GetClient() {
        if (this.cosmosClient != null) return this.cosmosClient;

        var options = new CosmosClientOptions {
            SerializerOptions = new CosmosSerializationOptions {
                PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
            }
        };
        CosmosClient client;
        if (this.credential != null) {
            client = new CosmosClient(this.config.EndpointUri, await this.credential.GetCredential(CancellationToken.None), options);
        } else {
            client = new CosmosClient(this.config.EndpointUri, this.config.PrimaryKey, options);
        }
        this.cosmosClient = client;
        return client;
    }

    private async Task<Container> GetContainer(string database, string container, CancellationToken cancellationToken) {
        CosmosClient client = await this.GetClient();
        Database cosmosDatabase = client.GetDatabase(database);
        return cosmosDatabase.GetContainer(container);
    }
}