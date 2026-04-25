namespace KServerTools.Common;

using Microsoft.Azure.Cosmos;

/// <summary>
/// Service for interacting with an Azure Cosmos DB account.
/// </summary>
/// <typeparam name="T">The Cosmos DB configuration type.</typeparam>
public interface IAzureCosmosDb<T> where T : IAzureCosmosDbConfiguration {
    /// <summary>
    /// Creates a database if it does not already exist.
    /// </summary>
    /// <param name="database">The name of the database to create.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the database was created; otherwise, <see langword="false"/>.</returns>
    Task<bool> CreateDatabaseAsync(string database, CancellationToken cancellationToken);

    /// <summary>
    /// Creates a container in the specified database if it does not already exist.
    /// </summary>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container to create.</param>
    /// <param name="partitionKey">The partition key path for the container.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the container was created; otherwise, <see langword="false"/>.</returns>
    Task<bool> CreateContainerAsync(string database, string container, string partitionKey, CancellationToken cancellationToken);

    /// <summary>
    /// Gets a single item from the specified container.
    /// </summary>
    /// <typeparam name="I">The type of the item to retrieve.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="itemId">The unique identifier of the item.</param>
    /// <param name="partitionKey">The partition key value of the item.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The item of type <typeparamref name="I"/>.</returns>
    Task<I> GetItemAsync<I>(string database, string container, string itemId, string partitionKey, CancellationToken cancellationToken) where I : ICosmosEntity;

    /// <summary>
    /// Queries items from the specified container using a SQL query string.
    /// </summary>
    /// <typeparam name="I">The type of the items to retrieve.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="query">The SQL query string.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An enumerable of items matching the query.</returns>
    Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, string query, CancellationToken cancellationToken) where I : ICosmosEntity;

    /// <summary>
    /// Queries items from the specified container using a <see cref="QueryDefinition"/>.
    /// </summary>
    /// <typeparam name="I">The type of the items to retrieve.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="queryDefinition">The parameterized query definition.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <param name="requestOptions">Optional request options for the query.</param>
    /// <returns>An enumerable of items matching the query.</returns>
    Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, QueryDefinition queryDefinition, CancellationToken cancellationToken, QueryRequestOptions? requestOptions = null) where I : ICosmosEntity;

    /// <summary>
    /// Adds an item to the specified container.
    /// </summary>
    /// <typeparam name="I">The type of the item to add.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="item">The item to add.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The added item of type <typeparamref name="I"/>.</returns>
    Task<I> AddItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity;

    /// <summary>
    /// Updates an existing item in the specified container.
    /// </summary>
    /// <typeparam name="I">The type of the item to update.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="item">The item with updated values.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The updated item of type <typeparamref name="I"/>.</returns>
    Task<I> UpdateItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity;

    /// <summary>
    /// Deletes an item from the specified container.
    /// </summary>
    /// <typeparam name="I">The type of the item to delete.</typeparam>
    /// <param name="database">The name of the database.</param>
    /// <param name="container">The name of the container.</param>
    /// <param name="item">The item to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I : ICosmosEntity;
}
