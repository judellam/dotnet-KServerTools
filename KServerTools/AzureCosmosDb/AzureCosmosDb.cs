namespace KServerTools.Common;

using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Internal Azure Cosmos DB service that provides CRUD operations against Cosmos DB containers.
/// </summary>
/// <typeparam name="T">The configuration type implementing <see cref="IAzureCosmosDbConfiguration"/>.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
/// <param name="configuration">The Cosmos DB configuration.</param>
/// <param name="credential">The token credential used for authentication.</param>
/// <param name="memoryCache">The memory cache for client reuse.</param>
/// <param name="logger">The JSON logger instance.</param>
internal class AzureCosmosDb<T, C>(T configuration, C credential, IMemoryCache memoryCache, IJsonLogger logger) : AzureServiceBase<T>(configuration, memoryCache, typeof(C).FullName ?? typeof(C).Name, logger), IAzureCosmosDb<T>, IDisposable where T : class, IAzureCosmosDbConfiguration where C : ITokenCredentialService {
    private readonly C credential = credential;
    private CosmosClient? cosmosClient;

    /// <summary>
    /// Creates a Cosmos DB database if it does not already exist.
    /// </summary>
    /// <param name="database">The name of the database to create.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the database was created; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> CreateDatabaseAsync(string database, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        return await this.LoggedOperationAsync($"Cosmos CreateDatabase {database}", async () => {
            CosmosClient client = await this.GetClient();
            DatabaseResponse response = await client.CreateDatabaseIfNotExistsAsync(database, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.StatusCode == HttpStatusCode.Created;
        }, cancellationToken);
    }

    /// <summary>
    /// Creates a container in the specified Cosmos DB database if it does not already exist.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container to create.</param>
    /// <param name="partitionKey">The partition key path for the container.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the container was created; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> CreateContainerAsync(string database, string container, string partitionKey, CancellationToken cancellationToken) {
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(partitionKey, nameof(partitionKey));
        return await this.LoggedOperationAsync($"Cosmos CreateContainer {database}/{container}", async () => {
            CosmosClient client = await this.GetClient();
            Database cosmosDatabase = client.GetDatabase(database);
            ContainerResponse response = await cosmosDatabase.CreateContainerIfNotExistsAsync(container, partitionKey, cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.StatusCode == HttpStatusCode.Created;
        }, cancellationToken);
    }

    /// <summary>
    /// Retrieves a single item from a Cosmos DB container by its identifier and partition key.
    /// </summary>
    /// <typeparam name="I">The entity type implementing <see cref="ICosmosEntity"/>.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="partitionKey">The partition key value.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The retrieved item of type <typeparamref name="I"/>.</returns>
    public async Task<I> GetItemAsync<I>(string database, string container, string itemId, string partitionKey, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(partitionKey, nameof(partitionKey));
        return await this.LoggedOperationAsync($"Cosmos GetItem {database}/{container}", async () => {
            Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
            ItemResponse<I> response = await cosmosContainer.ReadItemAsync<I>(itemId, new PartitionKey(partitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Resource;
        }, cancellationToken);
    }

    /// <summary>
    /// Queries items from a Cosmos DB container using a SQL query string.
    /// </summary>
    /// <typeparam name="I">The entity type implementing <see cref="ICosmosEntity"/>.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An enumerable of matching items.</returns>
    public async Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, string query, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNullOrEmpty(query, nameof(query));

        return await this.GetItemsAsync<I>(database, container, new QueryDefinition(query), cancellationToken);
    }

    /// <summary>
    /// Queries items from a Cosmos DB container using a <see cref="QueryDefinition"/>.
    /// </summary>
    /// <typeparam name="I">The entity type implementing <see cref="ICosmosEntity"/>.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="queryDefinition">The parameterized query definition.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="requestOptions">Optional query request options.</param>
    /// <returns>An enumerable of matching items.</returns>
    public async Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, QueryDefinition queryDefinition, CancellationToken cancellationToken, QueryRequestOptions? requestOptions = null) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        ArgumentNullException.ThrowIfNull(queryDefinition, nameof(queryDefinition));

        return await this.LoggedOperationAsync($"Cosmos Query {database}/{container}", async () => {
            Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
            using var iterator = cosmosContainer.GetItemQueryIterator<I>(queryDefinition, requestOptions: requestOptions);
            var results = new List<I>();
            while (iterator.HasMoreResults) {
                FeedResponse<I> response = await iterator.ReadNextAsync(cancellationToken).ConfigureAwait(false);
                results.AddRange(response);
            }

            return (IEnumerable<I>)results;
        }, cancellationToken);
    }

    /// <summary>
    /// Adds an item to a Cosmos DB container.
    /// </summary>
    /// <typeparam name="I">The entity type implementing <see cref="ICosmosEntity"/>.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The created item.</returns>
    public async Task<I> AddItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));

        return await this.LoggedOperationAsync($"Cosmos AddItem {database}/{container}", async () => {
            Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
            ItemResponse<I> response = await cosmosContainer.CreateItemAsync<I>(item, new PartitionKey(item.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Resource;
        }, cancellationToken);
    }

    /// <summary>
    /// Updates (upserts) an item in a Cosmos DB container.
    /// </summary>
    /// <typeparam name="I">The entity type implementing <see cref="ICosmosEntity"/>.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="item">The item to update.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The updated item.</returns>
    public async Task<I> UpdateItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        return await this.LoggedOperationAsync($"Cosmos UpdateItem {database}/{container}", async () => {
            Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
            ItemResponse<I> response = await cosmosContainer.UpsertItemAsync<I>(item, new PartitionKey(item.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
            return response.Resource;
        }, cancellationToken);
    }

    /// <summary>
    /// Deletes an item from a Cosmos DB container.
    /// </summary>
    /// <typeparam name="I">The entity type implementing <see cref="ICosmosEntity"/>.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="item">The item to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity {
        ArgumentNullException.ThrowIfNullOrEmpty(database, nameof(database));
        ArgumentNullException.ThrowIfNullOrEmpty(container, nameof(container));
        await this.LoggedOperationAsync($"Cosmos DeleteItem {database}/{container}", async () => {
            Container cosmosContainer = await this.GetContainer(database, container, cancellationToken).ConfigureAwait(false);
            await cosmosContainer.DeleteItemAsync<I>(item.Id, new PartitionKey(item.PartitionKey), cancellationToken: cancellationToken).ConfigureAwait(false);
        }, cancellationToken);
    }

    /// <summary>
    /// Disposes the underlying Cosmos DB client.
    /// </summary>
    public void Dispose() {
        if (this.cosmosClient != null) {
            this.cosmosClient.Dispose();
            this.cosmosClient = null;
        }
    }

    private async ValueTask<CosmosClient> GetClient() {
        if (this.cosmosClient != null) {
            return this.cosmosClient;
        }

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
