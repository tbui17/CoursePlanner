

namespace ViewModelTests.Utils;

public static class AsyncExtensions
{
    public static async Task<bool> WaitFor<T>(this T obj, Func<T, bool> predicate, int timeoutMs = 5000)
    {
        var cts = new CancellationTokenSource(timeoutMs);
        while (!cts.Token.IsCancellationRequested && !predicate(obj))
        {
            await Task.Delay(100);
        }

        return predicate(obj);
    }
}