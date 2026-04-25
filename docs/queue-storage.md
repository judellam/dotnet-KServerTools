# Queue Storage

KServerTools wraps Azure Queue Storage with two interfaces:

| Interface | Purpose |
|-----------|---------|
| `IAzureStorageQueueService<T>` | Enqueue, dequeue, delete messages and queues |
| `IAzureQueueManagementService<T>` | Peek, batch enqueue, count, and clear queues |

Both are registered automatically when you call `AddQueue<T>(...)`.

## Registration

```csharp
services.AddKServerTools(kst => kst
    .AddCommon()
    .AddQueue<MyQueueConfig>("AzureQueue")
);
```

## Configuration

```json
{
  "AzureQueue": {
    "AccountName": "mystorageaccount",
    "Endpoint": "queue.core.windows.net"
  }
}
```

```csharp
public class MyQueueConfig : IAzureStorageServiceConfig {
    public string AccountName { get; set; } = "";
    public string Endpoint { get; set; } = "";
}
```

## Usage — `IAzureStorageQueueService<T>`

### Enqueue a Message

```csharp
public class OrderProcessor(IAzureStorageQueueService<MyQueueConfig> queue) {

    public async Task SubmitOrderAsync(string orderId, CancellationToken ct) {
        await queue.EnqueMessageAsync("orders", orderId, ct);
    }
}
```

> **Note:** The method name `EnqueMessageAsync` is a known typo preserved for backward compatibility. See [API Compatibility Notes](#api-compatibility-notes).

### Dequeue Messages

```csharp
Message[] messages = await queue.DequeMessageAsync("orders", ct, messageCount: 5, visibilityTimeoutInSeconds: 30);

foreach (var msg in messages) {
    // Process msg.Body
    await queue.DeleteMessageAsync("orders", msg, ct);
}
```

### Delete a Queue

```csharp
bool deleted = await queue.DeleteQueueAsync("temp-queue", ct);
```

### Check if a Queue Exists

```csharp
bool exists = await queue.ExistsAsync("orders", ct);
```

## Usage — `IAzureQueueManagementService<T>`

### Peek Messages (Non-Destructive)

```csharp
public class QueueMonitor(IAzureQueueManagementService<MyQueueConfig> mgmt) {

    public async Task InspectAsync(CancellationToken ct) {
        Message[] peeked = await mgmt.PeekMessagesAsync("orders", maxMessages: 10, ct);
        foreach (var msg in peeked) {
            Console.WriteLine(msg.Body);
        }
    }
}
```

### Batch Enqueue

```csharp
var messages = new[] { "order-1", "order-2", "order-3" };
await mgmt.EnqueueBatchAsync("orders", messages, ct);
```

### Get Approximate Message Count

```csharp
int count = await mgmt.GetApproximateMessageCountAsync("orders", ct);
```

### Clear All Messages

```csharp
await mgmt.ClearMessagesAsync("orders", ct);
```

## Message Record

```csharp
public record Message(
    [property:JsonPropertyName("body")]       string Body,
    [property:JsonPropertyName("messageId")]  string MessageId,
    [property:JsonPropertyName("popReceipt")] string PopReceipt
);
```

## Interface Reference

### `IAzureStorageQueueService<T>`

```csharp
Task EnqueMessageAsync(string queueName, string message, CancellationToken ct);
Task<Message[]> DequeMessageAsync(string queueName, CancellationToken ct, int messageCount = 1, int visibilityTimeoutInSeconds = 5);
Task DeleteMessageAsync(string queueName, string messageId, string popReceipt, CancellationToken ct);
Task DeleteMessageAsync(string queueName, Message message, CancellationToken ct);
Task<bool> DeleteQueueAsync(string queueName, CancellationToken ct);
Task<bool> ExistsAsync(string queueName, CancellationToken ct);
```

### `IAzureQueueManagementService<T>`

```csharp
Task<Message[]> PeekMessagesAsync(string queueName, int maxMessages, CancellationToken ct);
Task EnqueueBatchAsync(string queueName, IEnumerable<string> messages, CancellationToken ct);
Task<int> GetApproximateMessageCountAsync(string queueName, CancellationToken ct);
Task ClearMessagesAsync(string queueName, CancellationToken ct);
```

## API Compatibility Notes

The method names `EnqueMessageAsync` and `DequeMessageAsync` contain spelling errors (`Enqueue` → `Enque`, `Dequeue` → `Deque`). These are preserved for backward compatibility. The newer `IAzureQueueManagementService<T>` uses the correct spelling (`EnqueueBatchAsync`).
