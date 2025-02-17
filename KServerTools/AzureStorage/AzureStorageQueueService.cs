namespace KServerTools.Common;

using System.Diagnostics;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;
using Microsoft.Extensions.Caching.Memory;

internal class AzureStorageQueueService<T, C>(T config, C credential, IJsonLogger logger) : AzureStorageBase<T,C>(config, credential), IAzureStorageQueueService<T> where T : IAzureStorageServiceConfig where C: ITokenCredentialService {
    private readonly IJsonLogger logger = logger;

    public async Task EnqueMessageAsync(string queueName, string message, CancellationToken cancellationToken) {
        Verify(queueName, message);
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool success = false;
        try {
            QueueClient queueClient = await this.GetQueueClient(queueName, true, cancellationToken)
                .ConfigureAwait(false);

            await queueClient.SendMessageAsync(message, cancellationToken)
                .ConfigureAwait(false);
            
            success = true;
        } finally {
            stopwatch.Stop();
            this.logger.Info($"{(success ? "Successfully" : "Unsuccessfully")} enqueued message to queue {queueName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<Message[]> DequeMessageAsync(string queueName, CancellationToken cancellationToken, int messageCount = 1, int visibilityTimeoutInSeconds = 300) {
        Verify(queueName);
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool success = false;

        try {
            QueueClient queueClient = await this.GetQueueClient(queueName, true, cancellationToken)
                .ConfigureAwait(false);

            QueueMessage[] messages = await queueClient.ReceiveMessagesAsync(
                messageCount,
                TimeSpan.FromSeconds(visibilityTimeoutInSeconds),
                cancellationToken).ConfigureAwait(false);

            if (messages.Length == 0) {
                return [];
            }

            return [.. messages.Select(m=>new Message(m.MessageText, m.MessageId, m.PopReceipt))];
        } finally {
            stopwatch.Stop();
            this.logger.Info($"{(success ? "Successfully" : "Unsuccessfully")} dequeued message from queue {queueName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task DeleteMessageAsync(string queueName, Message message, CancellationToken cancellationToken) =>
        await this.DeleteMessageAsync(queueName, message.MessageId, message.PopReceipt, cancellationToken);

    public async Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken cancellationToken) {
        Verify(queueName, messageId, popReceipt);

        Stopwatch stopwatch = Stopwatch.StartNew();
        bool success = false;

        try {
            QueueClient queueClient = await this.GetQueueClient(queueName, true, cancellationToken)
                .ConfigureAwait(false);

            await queueClient.DeleteMessageAsync(messageId, popReceipt, cancellationToken)
                .ConfigureAwait(false);
        } finally {
            stopwatch.Stop();
            this.logger.Info($"{(success ? "Successfully" : "Unsuccessfully")} deleted message from queue {queueName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<bool> DeleteQueueAsync(string queueName, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        bool success = false;
        try {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);

            return await queueClient.DeleteIfExistsAsync(cancellationToken)
                .ConfigureAwait(false);
        } finally {
            stopwatch.Stop();
            this.logger.Info($"{(success ? "Successfully" : "Unsuccessfully")} deleted queue {queueName}", stopwatch.ElapsedMilliseconds);
        }
    }

    public async Task<bool> ExistsAsync(string queueName, CancellationToken cancellationToken) {
        Stopwatch stopwatch = Stopwatch.StartNew();
        try {
            QueueClient queueClient = await this.GetQueueClient(queueName, false, cancellationToken)
                .ConfigureAwait(false);
            return await queueClient.ExistsAsync(cancellationToken)
                .ConfigureAwait(false);
        } finally {
            stopwatch.Stop();
            this.logger.Info($"Checked if queue {queueName} exists", stopwatch.ElapsedMilliseconds);
        }
    }

    protected async Task<QueueClient> GetQueueClient(string queueName, bool createIfNotExists, CancellationToken cancellationToken) {
        QueueClient client;
        string key = $"{this.config.AccountName}";
        if (!this.memoryCache.TryGetValue(key, out client!)) {
            Uri storageUri = new($"https://{config.AccountName}.{config.Endpoint}");
            QueueServiceClient serviceClient = new(storageUri, await this.credential.GetCredential(cancellationToken));

            client = serviceClient.GetQueueClient(queueName);

            if (createIfNotExists) {
                await client.CreateIfNotExistsAsync()
                    .ConfigureAwait(false);
            }

            this.memoryCache.Set(key, client, memoryCacheEntryOptions);
        }

        return client;
    }
}