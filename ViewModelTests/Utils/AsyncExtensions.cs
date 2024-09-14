using System.Diagnostics;
using System.Text;
using FluentAssertions;
using FluentAssertions.Execution;

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

    public static async Task<AndConstraint<T>> ShouldEventuallySatisfy<T>(this T subject, Action<T> assertion, int timeoutMs = 5000)
    {
        var scope = new AssertionScope();
        var cts = new CancellationTokenSource(timeoutMs);

        string[] errors = [];

        while (!cts.Token.IsCancellationRequested)
        {

            var scope2 = new AssertionScope();
            try
            {
                assertion(subject);
            }
            catch (NullReferenceException)
            {
                scope2.AddReportable("subject",() => subject?.ToString());
                var sb = new StringBuilder();

                sb
                    .AppendLine("Encountered null reference exception during assertion.")
                    .AppendLine("Subject: {subject}");

                scope2.FailWith(sb.ToString());
            }
            if (!scope2.HasFailures())
            {
                return new AndConstraint<T>(subject);
            }

            errors = scope2.Discard();
            await Task.Delay(100);
        }


        if (errors.Length > 0)
        {
            scope.FailWith(string.Join("\n", errors));
        }

        scope.FailWith("Expected assertion to pass within {0}ms, but it did not.", timeoutMs);
        scope.Dispose();
        throw new UnreachableException();
    }
}