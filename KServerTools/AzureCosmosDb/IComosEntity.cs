namespace KServerTools.Common;

/// <summary>
/// Represents an entity stored in Azure Cosmos DB.
/// </summary>
public interface ICosmosEntity : IEntity {
    /// <summary>
    /// Gets the unique identifier of the entity.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the partition key value for the entity.
    /// </summary>
    string PartitionKey { get; }
}

/// <summary>
/// Represents a lookup entity stored in Azure Cosmos DB.
/// </summary>
public interface ICosmosLookupEntity : IEntityLookup {
    /// <summary>
    /// Gets the unique identifier of the lookup entity.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the partition key value for the lookup entity.
    /// </summary>
    string PartitionKey { get; }
}
