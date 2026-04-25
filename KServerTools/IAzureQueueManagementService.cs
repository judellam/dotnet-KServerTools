namespace KServerTools.Common;

/// <summary>
/// Extended queue operations: peek, batch enqueue, message count, and clear.
/// </summary>
/// <typeparam name="T">The Azure Storage service configuration type.</typeparam>
/// <remarks>
/// Separated from <see cref="IAzureStorageQueueService{T}"/> to avoid breaking existing implementations.
/// </remarks>
public interface IAzureQueueManagementService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Peeks at messages without making them invisible to other consumers.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <param name="maxMessages">Number of messages to peek (1-32).</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>An array of peeked <see cref="Message"/> instances.</returns>
    Task<Message[]> PeekMessagesAsync(string queueName, int maxMessages, CancellationToken cancellationToken);

    /// <summary>
    /// Enqueues multiple messages to a queue.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <param name="messages">The collection of messages to enqueue.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task EnqueueBatchAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the approximate message count for a queue.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>The approximate number of messages in the queue.</returns>
    Task<int> GetApproximateMessageCountAsync(string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all messages from a queue.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken);
}
