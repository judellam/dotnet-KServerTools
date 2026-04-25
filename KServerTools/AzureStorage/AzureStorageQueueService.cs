namespace KServerTools.Common;

using System.Diagnostics;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Caching.Memory;

internal class AzureStorageQueueService<T, C>(T config, C credential, IJsonLogger logger, IMemoryCache memoryCache) : AzureStorageBase<T,C>(config, credential, memoryCache, logger), IAzureStorageQueueService<T> where T : class, IAzureStorageServiceConfig where C: ITokenCredentialService {

    public Task EnqueMessageAsync(string queueName, string message, CancellationToken cancellationToken) {
        Verify(queueName, message);
        return this.LoggedOperationAsync($"Enqueued message to queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, true, cancellationToken)
                .ConfigureAwait(false);
            await queueClient.SendMessageAsync(message, cancellationToken)
                .ConfigureAwait(false);
        });
    }

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

    public async Task DeleteMessageAsync(string queueName, Message message, CancellationToken cancellationToken) =>
        await this.DeleteMessageAsync(queueName, message.MessageId, message.PopReceipt, cancellationToken);

    public Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken) {
        Verify(queueName, messageId, popReceipt);
        return this.LoggedOperationAsync($"Deleted message from queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken)
                .ConfigureAwait(false);
        });
    }

    public Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken) =>
        this.LoggedOperationAsync($"Deleted queue {queueName}", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            var response = await queueClient.DeleteIfExistsAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.Value;
        });

    public Task<bool> ExistsAsync(string queueName, CancellationToken cancellationToken) =>
        this.LoggedOperationAsync($"Checked if queue {queueName} exists", async () => {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            var response = await queueClient.ExistsAsync(cancellationToken)
                .ConfigureAwait(false);
            return response.Value;
        });

    protected async Task<QueueClient> GetQueueClient(string queueName, bool createIfNotExists, CancellationToken cancellationToken) {
        string key = $"queue:{this.config.AccountName}:{queueName}";
        return await this.GetOrCreateCachedAsync(key, async () => {
            Uri storageUri = new($"https://{config.AccountName}.{config.Endpoint}");
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