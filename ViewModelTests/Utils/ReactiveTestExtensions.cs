using ReactiveUI;

namespace ViewModelTests.Utils;

public static class ReactiveTestExtensions
{
    public static async Task<bool> WaitFor<T>(this T model, Func<T, bool> predicate, int timeoutMs = 5000)
        where T : IReactiveObject
    {
        var cts = new CancellationTokenSource(timeoutMs);
        while (!cts.Token.IsCancellationRequested && !predicate(model))
        {
            await Task.Delay(100, cts.Token);
        }

        return predicate(model);
    }
}