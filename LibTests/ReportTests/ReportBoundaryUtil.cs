using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;

namespace LibTests.ReportTests;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public class ReportBoundaryUtil(IDurationReport report)
{
    private readonly IDurationReport _report = report.Should().BeAssignableTo<IDurationReport>().Subject;

    [AttributeUsage(AttributeTargets.Method)]
    public class SubAssertionAttribute : Attribute;


    [SubAssertion]
    public void AssertMinBoundaries()
    {
        using var scope = new AssertionScope();
        _report.CompletedTime.Should().BeGreaterThanOrEqualTo(default);
        _report.AverageDuration.Should().BeGreaterThanOrEqualTo(default);
        _report.RemainingTime.Should().BeGreaterThanOrEqualTo(default);
        _report.TotalTime.Should().BeGreaterThanOrEqualTo(default);
        _report.CompletedItems.Should().BeGreaterThanOrEqualTo(default);
        _report.PercentComplete.Should().BeGreaterThanOrEqualTo(default);
        _report.PercentRemaining.Should().BeGreaterThanOrEqualTo(default);
        _report.RemainingItems.Should().BeGreaterThanOrEqualTo(default);
        _report.TotalItems.Should().BeGreaterThanOrEqualTo(default);
        _report.MaxDate.Should().BeOnOrAfter(default);
        _report.MinDate.Should().BeOnOrAfter(default);
    }

    [SubAssertion]
    public void AssertRelationalBoundaries()
    {
        using var _ = new AssertionScope();
        _report.CompletedItems.Should().BeLessThanOrEqualTo(_report.TotalItems);
        _report.AverageDuration.Should().BeLessThanOrEqualTo(_report.TotalTime);
        _report.CompletedTime.Should().BeLessThanOrEqualTo(_report.TotalTime);
        _report.MaxDate.Should().BeOnOrAfter(_report.MinDate);
        new[] { _report.PercentComplete, _report.PercentRemaining }.Sum().Should().BeOneOf(default, 100);
    }

    public IEnumerable<MethodInfo> GetSubAssertions()
    {
        return GetType()
            .GetMethods()
            .Where(x => x.GetCustomAttribute<SubAssertionAttribute>() is not null);
    }

    public void AssertIDurationBoundaries()
    {
        foreach (var method in GetType()
                     .GetMethods()
                     .Where(x => x.GetCustomAttribute<SubAssertionAttribute>() is not null)
                )
        {
            method.Invoke(this, null);
        }
    }
}