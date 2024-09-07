using System.Linq.Expressions;
using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Execution;
using Moq;

namespace BaseTestSetup.Lib;

public static class MockExtensions
{
    public static AndWhichConstraint<GenericCollectionAssertions<IInvocation>, IInvocation> ShouldCall<T>(
        this Mock<T> mock, Expression<Func<T, string>> expr) where T : class
    {
        var expr2 = (ConstantExpression)expr.Body;

        var str = (string)expr2.Value!;

        return mock.Invocations.Should().Contain(x => x.Method.Name == str);
    }

    public static AndConstraint<GenericCollectionAssertions<IInvocation>> ShouldNotCall<T>(
        this Mock<T> mock, string methodName) where T : class
    {
        return mock.Invocations.Should().NotContain(x => x.Method.Name == methodName);
    }

    public static AndConstraint<GenericCollectionAssertions<IInvocation>> ShouldCall<T>(this Mock<T> mock)
        where T : class
    {
        using var scope = new AssertionScope();
        var mockInvocations = mock.Invocations.Where(x => x.Method.DeclaringType?.Name is { } s &&
                                                          (s.StartsWith("Action") || s.StartsWith("Func")));

        return mockInvocations.Should().NotBeEmpty($"{mock} should call a Func or Action.");
    }
}
