namespace KServerTools.Common;

public interface IComosEntity: IEntity {
    string Id { get; }
    string PartitionKey { get; }
}

public interface IComosEntityLookup : IEntityLookup {
    string Id { get; }
    string PartitionKey { get; }
}