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
/// <typeparam name="M">The Model that function call will interact or return</typeparam>
/// <typeparam name="L">The look up model. They can be the same, but the look up model is usually a smaller record.</typeparam>
public interface IRepository<M, L> where M : IEntity where L : IEntityLookup {
    Task<M> GetAsync(L lookup, CancellationToken cancellationToken);
    Task<bool> CreateOrUpdateAsync(M model, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(L lookup, CancellationToken cancellationToken);
}