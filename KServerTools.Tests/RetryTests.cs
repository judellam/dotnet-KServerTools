namespace KServerTools.Tests;

using KServerTools.Common;

public class RetryTests {
    [Fact]
    public async Task DoAsync_SucceedsOnFirstAttempt() {
        int callCount = 0;
        await Retry.DoAsync(async () => {
            callCount++;
            await Task.CompletedTask;
        }, maxRetries: 3, delay: 10);

        Assert.Equal(1, callCount);
    }

    [Fact]
    public async Task DoAsync_RetriesOnFailureThenSucceeds() {
        int callCount = 0;
        await Retry.DoAsync(async () => {
            callCount++;
            if (callCount < 3) throw new InvalidOperationException("fail");
            await Task.CompletedTask;
        }, maxRetries: 3, delay: 10);

        Assert.Equal(3, callCount);
    }

    [Fact]
    public async Task DoAsync_ThrowsAggregateAfterAllRetriesExhausted() {
        int callCount = 0;
        var ex = await Assert.ThrowsAsync<AggregateException>(async () => {
            await Retry.DoAsync(async () => {
                callCount++;
                await Task.CompletedTask;
                throw new InvalidOperationException($"fail {callCount}");
            }, maxRetries: 3, delay: 10);
        });

        Assert.Equal(3, callCount);
        Assert.Equal(3, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task DoAsync_DefaultMaxRetries_IsThree() {
        int callCount = 0;
        var ex = await Assert.ThrowsAsync<AggregateException>(async () => {
            await Retry.DoAsync(async () => {
                callCount++;
                await Task.CompletedTask;
                throw new Exception("fail");
            }, delay: 10);
        });

        Assert.Equal(3, callCount);
    }
}
