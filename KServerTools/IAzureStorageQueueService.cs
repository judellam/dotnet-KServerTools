namespace KServerTools.Common;

using System.Text.Json.Serialization;

public record Message(
    [property:JsonPropertyName("body")] string Body, 
    [property:JsonPropertyName("messageId")] string MessageId, 
    [property:JsonPropertyName("popReceipt")] string PopReceipt);

public interface IAzureStorageQueueService<T> where T : IAzureStorageServiceConfig {
    Task EnqueMessageAsync(string queueName, string message, CancellationToken cancellationToken);
    Task<Message[]> DequeMessageAsync(string queueName, CancellationToken cancellationToken, int messageCount = 1, int visibilityTimeoutInSeconds = 5);
    Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken);
    Task DeleteMessageAsync(string queueName, Message message, CancellationToken cancellationToken);
    Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string queueName, CancellationToken cancellationToken);
}