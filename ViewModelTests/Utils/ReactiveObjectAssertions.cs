using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows.Input;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using Lib.Utils;
using ReactiveUI;

namespace ViewModelTests.Utils;

public class ReactiveObjectAssertions<T>(T instance) :
    ReferenceTypeAssertions<T, ReactiveObjectAssertions<T>>(instance) where T : IReactiveObject
{
    protected override string Identifier => "";

    [CustomAssertion]
    public async Task<AndConstraint<ReactiveObjectAssertions<T>>> EventuallyHave(
        Func<T, bool> predicate, int timeoutMs = 5000)
    {
        await Subject.WaitFor(predicate, timeoutMs);


        Execute.Assertion
            .Given(() => Subject)
            .ForCondition(predicate)
            .FailWith(
                "Expected {0} to match the predicate, but it did not.\nState:\n{1}",
                x => x.GetType().Name,
                ctx => ctx
                    .GetType()
                    .GetProperties()
                    .Where(x => !Statics.Filter(x))
                    .ToDictionary(x => x.Name, x => x.GetValue(ctx)));

        return new AndConstraint<ReactiveObjectAssertions<T>>(this);
    }

    [CustomAssertion]
    public async Task<AndConstraint<ReactiveObjectAssertions<T>>> EventuallySatisfy(
        Action<T> assertion, int timeoutMs = 5000)
    {
        var scope = new AssertionScope();
        var cts = new CancellationTokenSource(timeoutMs);

        string[] errors = [];

        while (!cts.Token.IsCancellationRequested)
        {
            var scope2 = new AssertionScope();
            assertion(Subject);
            if (!scope2.HasFailures())
            {
                return new AndConstraint<ReactiveObjectAssertions<T>>(this);
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

    [CustomAssertion]
    public async Task<AndConstraint<ReactiveObjectAssertions<T>>> EventuallySatisfy<T2>(
        Expression<Func<T, T2?>> selector, Action<T2> assertion, int timeoutMs = 5000) where T2 : class
    {
        var scope = new AssertionScope();
        var cts = new CancellationTokenSource(timeoutMs);

        string[] errors = [];

        var compiledSelector = selector.Compile();

        while (!cts.Token.IsCancellationRequested)
        {
            var scope2 = new AssertionScope();
            var subj = compiledSelector(Subject);
            if (subj is null)
            {
                scope2.FailWith("Expected {0} to be not null, but it was.", selector);
            }
            else
            {
                assertion(subj);
            }

            if (!scope2.HasFailures())
            {
                return new AndConstraint<ReactiveObjectAssertions<T>>(this);
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

file static class Statics
{
    public static readonly Func<PropertyInfo, bool> Filter = new List<Func<PropertyInfo, bool>>
    {
        x => x.PropertyType.IsAssignableTo(typeof(ICommand)),
        x => x.Name is "Changing" or "Changed" or "ThrownExceptions",
        x => x.PropertyType.IsAssignableTo(typeof(ReactiveCommand)),
        x => x.PropertyType.IsAssignableTo(typeof(Delegate))
    }.ToAnyPredicate();
}

public static class ReactiveObjectAssertionsExtensions
{
    public static ReactiveObjectAssertions<T> Should<T>(this T instance) where T : IReactiveObject =>
        new(instance);
}