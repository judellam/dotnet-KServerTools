namespace KServerTools.Common;

public interface IComosEntity: IEntity {
    string Id { get; }
    string PartitionKey { get; }
}