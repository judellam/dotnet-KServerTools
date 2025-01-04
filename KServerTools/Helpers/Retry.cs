namespace KServerTools.Common;

public static class Retry {
    public static async Task DoAsync(Func<Task> action, int maxRetries = 3, int delay = 1000) {
        var exceptions = new List<Exception>();
        for (var i = 0; i < maxRetries; i++) {
            try {
                await action();
                return;
            }
            catch (Exception ex) {
                exceptions.Add(ex);
                await Task.Delay(delay + (i * 17))
                    .ConfigureAwait(false);
            }
        }
        throw new AggregateException(exceptions);
    }
}