namespace KServerTools.Common;

using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Caching.Memory;

/// <summary>
/// Azure Storage queue service that provides queue messaging operations with logging.
/// </summary>
/// <typeparam name="T">The configuration type implementing <see cref="IAzureStorageServiceConfig"/>.</typeparam>
/// <typeparam name="C">The credential type implementing <see cref="ITokenCredentialService"/>.</typeparam>
/// <param name="config">The storage service configuration.</param>
/// <param name="credential">The token credential used for authentication.</param>
/// <param name="logger">The JSON logger instance.</param>
/// <param name="memoryCache">The memory cache for client reuse.</param>
internal class AzureStorageQueueService<T, C>(T config, C credential, IJsonLogger logger, IMemoryCache memoryCache) : AzureStorageBase<T, C>(config, credential, memoryCache, logger), IAzureStorageQueueService<T>, IAzureQueueManagementService<T> where T : class, IAzureStorageServiceConfig where C : ITokenCredentialService {
    /// <summary>
    /// Enqueues a message to the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="message">The message content to enqueue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task EnqueMessageAsync(string queueName, string message, CancellationToken cancellationToken) {
        Verify(queueName, message);
        return this.LoggedOperationAsync($"Enqueued message to queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, true, cancellationToken)
                .ConfigureAwait(false);
            await queueClient.SendMessageAsync(message, cancellationToken)
                .ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Dequeues messages from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <param name="messageCount">The number of messages to dequeue.</param>
    /// <param name="visibilityTimeoutInSeconds">The visibility timeout in seconds.</param>
    /// <returns>An array of dequeued messages.</returns>
    public Task<Message[]> DequeMessageAsync(string queueName, CancellationToken cancellationToken, int messageCount = 1, int visibilityTimeoutInSeconds = 300) {
        Verify(queueName);
        return this.LoggedOperationAsync($"Dequeued message from queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);

            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(
                messageCount,
                TimeSpan.FromSeconds(visibilityTimeoutInSeconds),
                cancellationToken).ConfigureAwait(false);

            if (messages.Length == 0) {
                return Array.Empty<Message>();
            }

            return [.. messages.Select(m => new Message(m.MessageText, m.MessageId, m.PopReceipt))];
        });
    }

    /// <summary>
    /// Deletes a message from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="message">The message to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public async Task DeleteMessageAsync(string queueName, Message message, CancellationToken cancellationToken) =>
        await this.DeleteMessageAsync(queueName, message.MessageId, message.PopReceipt, cancellationToken);

    /// <summary>
    /// Deletes a message from the specified queue by message identifier and pop receipt.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="messageId">The identifier of the message to delete.</param>
    /// <param name="popReceipt">The pop receipt of the message.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken) {
        Verify(queueName, messageId, popReceipt);
        return this.LoggedOperationAsync($"Deleted message from queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken)
                .ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Deletes the specified queue if it exists.
    /// </summary>
    /// <param name="queueName">The name of the queue to delete.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the queue was deleted; otherwise, <see langword="false"/>.</returns>
    public Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken) =>
        this.LoggedOperationAsync($"Deleted queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            var response = await queueClient.DeleteIfExistsAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.Value;
        });

    /// <summary>
    /// Checks whether the specified queue exists.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns><see langword="true"/> if the queue exists; otherwise, <see langword="false"/>.</returns>
    public Task<bool> ExistsAsync(string queueName, CancellationToken cancellationToken) =>
        this.LoggedOperationAsync($"Checked if queue {queueName} exists", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            var response = await queueClient.ExistsAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.Value;
        });

    /// <summary>
    /// Peeks at messages in the specified queue without removing them.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="maxMessages">The maximum number of messages to peek.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>An array of peeked messages.</returns>
    public Task<Message[]> PeekMessagesAsync(string queueName, int maxMessages, CancellationToken cancellationToken) {
        Verify(queueName);
        return this.LoggedOperationAsync($"Peeked {maxMessages} message(s) from queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);

            PeekedMessage[] peeked = await queueClient.PeekMessagesAsync(maxMessages, cancellationToken)
                .ConfigureAwait(false);

            if (peeked.Length == 0) {
                return Array.Empty<Message>();
            }

            return peeked.Select(m => new Message(m.MessageText, m.MessageId, string.Empty)).ToArray();
        });
    }

    /// <summary>
    /// Enqueues a batch of messages to the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="messages">The messages to enqueue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task EnqueueBatchAsync(string queueName, IEnumerable<string> messages, CancellationToken cancellationToken) {
        Verify(queueName);
        return this.LoggedOperationAsync($"Batch enqueue to queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, true, cancellationToken)
                .ConfigureAwait(false);

            foreach (var message in messages) {
                cancellationToken.ThrowIfCancellationRequested();
                await queueClient.SendMessageAsync(message, cancellationToken)
                    .ConfigureAwait(false);
            }
        });
    }

    /// <summary>
    /// Gets the approximate number of messages in the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>The approximate message count.</returns>
    public Task<int> GetApproximateMessageCountAsync(string queueName, CancellationToken cancellationToken) {
        Verify(queueName);
        return this.LoggedOperationAsync($"Get message count for queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);

            QueueProperties properties = await queueClient.GetPropertiesAsync(cancellationToken)
                .ConfigureAwait(false);

            return properties.ApproximateMessagesCount;
        });
    }

    /// <summary>
    /// Clears all messages from the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
    public Task ClearMessagesAsync(string queueName, CancellationToken cancellationToken) {
        Verify(queueName);
        return this.LoggedOperationAsync($"Cleared all messages from queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            await queueClient.ClearMessagesAsync(cancellationToken)
                .ConfigureAwait(false);
        });
    }

    /// <summary>
    /// Gets or creates a cached <see cref="QueueClient"/> for the specified queue.
    /// </summary>
    /// <param name="queueName">The name of the queue.</param>
    /// <param name="createIfNotExists">Whether to create the queue if it does not exist.</param>
    /// <param name="cancellationToken">A token to cancel the operation.</param>
    /// <returns>A <see cref="QueueClient"/> for the queue.</returns>
    protected async Task<QueueClient> GetQueueClient(string queueName, bool createIfNotExists, CancellationToken cancellationToken) {
        string key = $"queue:{this.config.AccountName}:{queueName}";
        return await this.GetOrCreateCachedAsync(key, async () => {
            Uri storageUri = new($"https://{this.config.AccountName}.{this.config.Endpoint}");
            QueueServiceClient serviceClient = new(storageUri, await this.credential.GetCredential(cancellationToken));

            var client = serviceClient.GetQueueClient(queueName);

            if (createIfNotExists) {
                await client.CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            return client;
        }).ConfigureAwait(false);
    }
}
