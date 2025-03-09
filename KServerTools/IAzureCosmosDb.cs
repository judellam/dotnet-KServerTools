namespace KServerTools.Common;

public interface IAzureCosmosDb<T> where T : IAzureCosmosDbConfiguration {
    Task<bool> CreateDatabaseAsync(string database, CancellationToken cancellationToken);
    Task<bool> CreateContainerAsync(string database, string container, string partitionKey, CancellationToken cancellationToken);
    Task<I> GetItemAsync<I>(string database, string container, string itemId, string partitionKey, CancellationToken cancellationToken) where I: IComosEntity;
    Task<IEnumerable<I>> GetItemsAsync<I>(string database, string container, string query, CancellationToken cancellationToken) where I: IComosEntity;
    Task<I> AddItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I: IComosEntity;
    Task<I> UpdateItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I: IComosEntity;
    Task DeleteItemAsync<I>(string database, string container, I item, CancellationToken cancellationToken) where I: IComosEntity;
}