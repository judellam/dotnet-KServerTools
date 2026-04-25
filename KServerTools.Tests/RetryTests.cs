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

    [Fact]
    public async Task DoAsync_Generic_ReturnsValue() {
        var result = await Retry.DoAsync(async () => {
            await Task.CompletedTask;
            return 42;
        }, maxRetries: 3, delay: 10);

        Assert.Equal(42, result);
    }

    [Fact]
    public async Task DoAsync_Generic_RetriesAndReturns() {
        int callCount = 0;
        var result = await Retry.DoAsync(async () => {
            callCount++;
            if (callCount < 2) throw new InvalidOperationException("fail");
            await Task.CompletedTask;
            return "success";
        }, maxRetries: 3, delay: 10);

        Assert.Equal("success", result);
        Assert.Equal(2, callCount);
    }

    [Fact]
    public async Task DoAsync_Generic_ThrowsAfterExhausted() {
        var ex = await Assert.ThrowsAsync<AggregateException>(async () => {
            await Retry.DoAsync<int>(async () => {
                await Task.CompletedTask;
                throw new Exception("fail");
            }, maxRetries: 2, delay: 10);
        });

        Assert.Equal(2, ex.InnerExceptions.Count);
    }

    [Fact]
    public async Task DoAsync_ShouldRetry_SkipsNonRetryableExceptions() {
        int callCount = 0;
        var ex = await Assert.ThrowsAsync<AggregateException>(async () => {
            await Retry.DoAsync(async () => {
                callCount++;
                await Task.CompletedTask;
                throw new ArgumentException("not retryable");
            }, maxRetries: 3, delay: 10, shouldRetry: ex => ex is not ArgumentException);
        });

        Assert.Equal(1, callCount);
        Assert.Single(ex.InnerExceptions);
    }

    [Fact]
    public async Task DoAsync_ShouldRetry_RetriesOnlyMatchingExceptions() {
        int callCount = 0;
        await Retry.DoAsync(async () => {
            callCount++;
            if (callCount < 3) throw new TimeoutException("transient");
            await Task.CompletedTask;
        }, maxRetries: 3, delay: 10, shouldRetry: ex => ex is TimeoutException);

        Assert.Equal(3, callCount);
    }

    [Fact]
    public void ComputeDelay_AppliesExponentialBackoff() {
        int delay0 = Retry.ComputeDelay(1000, 0);
        int delay1 = Retry.ComputeDelay(1000, 1);
        int delay2 = Retry.ComputeDelay(1000, 2);

        // attempt 0: range [500, 1000), attempt 1: [1000, 2000), attempt 2: [2000, 4000)
        Assert.InRange(delay0, 500, 999);
        Assert.InRange(delay1, 1000, 1999);
        Assert.InRange(delay2, 2000, 3999);
    }

    [Fact]
    public void ComputeDelay_CapsAtMaxAttempt() {
        // attempt 8 = 1000 * 256 = 256000, attempt 20 should be same (capped at 8)
        int delay8 = Retry.ComputeDelay(1000, 8);
        int delay20 = Retry.ComputeDelay(1000, 20);

        int maxCapped = 1000 * (1 << 8); // 256000
        Assert.InRange(delay8, maxCapped / 2, maxCapped - 1);
        Assert.InRange(delay20, maxCapped / 2, maxCapped - 1);
    }

    [Fact]
    public void ComputeDelay_HasJitter_NotAllSame() {
        var delays = Enumerable.Range(0, 20).Select(_ => Retry.ComputeDelay(1000, 2)).ToList();
        // With jitter, not all values should be identical
        Assert.True(delays.Distinct().Count() > 1, "Expected jitter to produce varying delays");
    }
}
