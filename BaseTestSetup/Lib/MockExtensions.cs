using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;
using Moq;
using Moq.Language.Flow;

namespace BaseTestSetup.Lib;

public static class MockExtensions
{
    private static string GetExpressionConst<T>(this Expression<Func<T, string>> expr)
    {
        var expr2 = (ConstantExpression)expr.Body;

        var str = (string)expr2.Value!;
        return str;
    }

    public static AndWhichConstraint<GenericCollectionAssertions<IInvocation>, IInvocation> ShouldCall<T>(
        this Mock<T> mock,
        Expression<Func<T, string>> expr
    ) where T : class
    {
        var str = expr.GetExpressionConst();
        return mock.Invocations.Should().Contain(x => x.Method.Name == str);
    }

    public static AndConstraint<GenericCollectionAssertions<IInvocation>> ShouldCall<T>(
        this Mock<T> mock,
        Expression<Func<T, string>> expr,
        int times
    ) where T : class
    {
        var expr2 = (ConstantExpression)expr.Body;

        var str = (string)expr2.Value!;

        return mock.Invocations.Where(x => x.Method.Name == str).Should().HaveCount(times);
    }

    public static AndConstraint<GenericCollectionAssertions<IInvocation>> ShouldNotCall<T>(
        this Mock<T> mock,
        string methodName
    ) where T : class
    {
        return mock.Invocations.Should().NotContain(x => x.Method.Name == methodName);
    }

    public static AndConstraint<GenericCollectionAssertions<IInvocation>> ShouldCall<T>(this Mock<T> mock)
        where T : class
    {
        using var scope = new AssertionScope();
        var mockInvocations = mock.Invocations.Where(x => x.Method.DeclaringType?.Name is { } s &&
                                                          (s.StartsWith("Action") || s.StartsWith("Func"))
        );

        return mockInvocations.Should().NotBeEmpty($"{mock} should call a Func or Action.");
    }

    public static ISetup<T> SetupDefaultArgs<T>(
        this Mock<T> mock,
        Expression<Func<T, string>> expr
    )
        where T : class
    {
        return mock.SetupDefaultArgs(expr.GetExpressionConst());
    }


    public static ISetup<T> SetupDefaultArgs<T>(this Mock<T> mock, string methodName)
        where T : class
    {
        var method = typeof(T).GetMethod(methodName);
        if (method == null)
            throw new ArgumentException($"No method named '{methodName}' exists on type '{typeof(T).Name}'");

        var instance = Expression.Parameter(typeof(T), "m");
        var callExp = Expression.Call(instance,
            method,
            method.GetParameters().Select(p => GenerateItIsAny(p.ParameterType))
        );
        var exp = Expression.Lambda<Action<T>>(callExp, instance);
        return mock.Setup(exp);

        static MethodCallExpression GenerateItIsAny(Type T)
        {
            var itIsAnyT = typeof(It)
                .GetMethod("IsAny")!
                .MakeGenericMethod(T);
            return Expression.Call(itIsAnyT);
        }
    }
}