namespace KServerTools.Common;

/// <summary>
/// Configuration for connecting to an Azure Cosmos DB account.
/// </summary>
public interface IAzureCosmosDbConfiguration {
    /// <summary>
    /// Gets or sets the endpoint URI of the Cosmos DB account.
    /// </summary>
    string EndpointUri { get; set; }

    /// <summary>
    /// Gets or sets the primary key for the Cosmos DB account.
    /// </summary>
    string PrimaryKey { get; set; }
}

/// <summary>
/// Default implementation of <see cref="IAzureCosmosDbConfiguration"/>.
/// </summary>
public class AzureCosmosDbConfiguration : IAzureCosmosDbConfiguration {
    private string? primaryKey;

    /// <inheritdoc/>
    public required string EndpointUri { get; set; }

    /// <inheritdoc/>
    public string PrimaryKey {
        get {
            return this.primaryKey ?? string.Empty;
        }
        set {
            this.primaryKey = value;
        }
    }

    /// <summary>
    /// Gets the secret value for the Cosmos DB primary key.
    /// </summary>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The primary key value, or an empty string if not set.</returns>
    public virtual Task<string> GetSecret(CancellationToken cancellationToken) {
        return Task.FromResult(this.primaryKey ?? string.Empty);
    }
}
