namespace KServerTools.Common;

/// <summary>
/// Extended queue operations: peek, batch enqueue, message count, and clear.
/// </summary>
/// <remarks>
/// Separated from <see cref="IAzureStorageQueueService{T}"/> to avoid breaking existing implementations.
/// </remarks>
public interface IAzureQueueManagementService<T> where T : IAzureStorageServiceConfig {
    /// <summary>
    /// Peeks at messages without making them invisible to other consumers.
    /// </summary>
    /// <param name="queueName">The queue name.</param>
    /// <param name="maxMessages">Number of messages to peek (1-32).</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<Message[]> PeekMessagesAsync(string queueName, int maxMessages, CancellationToken cancellationToken);

    /// <summary>
    /// Enqueues multiple messages to a queue.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task EnqueueBatchAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken);

    /// <summary>
    /// Gets the approximate message count for a queue.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task<int> GetApproximateMessageCountAsync(string queueName, CancellationToken cancellationToken);

    /// <summary>
    /// Clears all messages from a queue.
    /// </summary>
    /// <returns><placeholder>A <see cref="Task"/> representing the asynchronous operation.</placeholder></returns>
    Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken);
}
