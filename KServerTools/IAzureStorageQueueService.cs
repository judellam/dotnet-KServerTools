namespace KServerTools.Common;

using System.Text.Json.Serialization;

/// <summary>
/// Service for interacting with Azure Storage queues.
/// </summary>
/// <typeparam name="T">The Azure Storage service configuration type.</typeparam>
public interface IAzureStorageQueueService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Enqueues a message to the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="message">The message content to enqueue.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task EnqueMessageAsync(string queueName, string message, CancellationToken cancellationToken);

    /// <summary>
    /// Dequeues messages from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <param name="messageCount">The number of messages to dequeue.</param>
    /// <param name="visibilityTimeoutInSeconds">The visibility timeout in seconds for dequeued messages.</param>
    /// <returns>An array of <see cref="Message"/> instances dequeued from the queue.</returns>
    Task<Message[]> DequeMessageAsync(string queueName, CancellationToken cancellationToken, int messageCount = 1, int visibilityTimeoutInSeconds = 5);

    /// <summary>
    /// Deletes a message from the specified queue by message identifier and pop receipt.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="messageId">The unique identifier of the message to delete.</param>
    /// <param name="popReceipt">The pop receipt of the message to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes a message from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="message">The <see cref="Message"/> to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task DeleteMessageAsync(string queueName, Message message, CancellationToken cancellationToken);

    /// <summary>
    /// Deletes the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue to delete.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the queue was deleted; otherwise, <see langword="false"/>.</returns>
    Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Checks whether the specified queue exists.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns><see langword="true"/> if the queue exists; otherwise, <see langword="false"/>.</returns>
    Task<bool> ExistsAsync(string queueName, CancellationToken cancellationToken);
}

/// <summary>
/// Represents a message retrieved from an Azure Storage queue.
/// </summary>
/// <param name="Body">The message body.</param>
/// <param name="MessageId">The unique identifier of the message.</param>
/// <param name="PopReceipt">The pop receipt used to delete or update the message.</param>
public record Message(
    [property: JsonPropertyName("body")] string Body,
    [property: JsonPropertyName("messageId")] string MessageId,
    [property: JsonPropertyName("popReceipt")] string PopReceipt);
