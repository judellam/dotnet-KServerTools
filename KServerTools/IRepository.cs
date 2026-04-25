namespace KServerTools.Common;

/// <summary>
/// Interface for an entity. This is a marker interface.
/// </summary>
public interface IEntity {
}

/// <summary>
/// Interface for an entity lookup. This is a marker interface.
/// </summary>
public interface IEntityLookup {
}

/// <summary>
/// Simple repository interface for CRUD operations. Overlay this interface on top of a SQL or NoSQL database
/// for a consistent way to interact with the database - and ability to swap out the database implementation.
/// </summary>
/// <typeparam name="M">The Model that function call will interact or return.</typeparam>
/// <typeparam name="L">The look up model. They can be the same, but the look up model is usually a smaller record.</typeparam>
public interface IRepository<M, L> where M : class, IEntity where L : class, IEntityLookup {
    /// <summary>
    /// Gets an entity by the specified lookup.
    /// </summary>
    /// <param name="lookup">The lookup criteria.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The entity if found; otherwise, <see langword="null"/>.</returns>
    Task<M?> GetAsync(L lookup, CancellationToken cancellationToken);

    /// <summary>
    /// Creates or updates an entity.
    /// </summary>
    /// <param name="model">The entity to create or update.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the operation succeeded; otherwise, <see langword="false"/>.</returns>
    Task<bool> CreateOrUpdateAsync(M model, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes an entity by the specified lookup.
    /// </summary>
    /// <param name="lookup">The lookup criteria identifying the entity to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the entity was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteAsync(L lookup, CancellationToken cancellationToken);
}

/// <summary>
/// Additional interface for the respository to pull multiple records and extend the core IRepository interface.
/// </summary>
/// <typeparam name="M">The Model that function call will interact or return.</typeparam>
/// <typeparam name="L">The look up model. They can be the same, but the look up model is usually a smaller record.</typeparam>
public interface IGetMultiple<M, L> where M : class, IEntity where L : class, IEntityLookup {
    /// <summary>
    /// Gets multiple entities matching the specified lookup.
    /// </summary>
    /// <param name="lookup">The lookup criteria.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An enumerable of matching entities.</returns>
    Task<IEnumerable<M?>> GetMultipleAsync(L lookup, CancellationToken cancellationToken);
}
