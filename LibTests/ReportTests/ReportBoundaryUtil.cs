using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;
using Lib.Utils;

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

    public void AssertSubReportRelationalBoundaries()
    {
        var data = _report.Should().BeOfType<AggregateDurationReport>().Subject;
        using var scope = new AssertionScope();
        var sub = data.SubReports.Values().ToList();


        data.AverageDuration.Should()
            .BeLessThanOrEqualTo(sub.Max(x => x.AverageDuration), nameof(data.AverageDuration))
            .And.BeGreaterThanOrEqualTo(sub.Min(x => x.AverageDuration), nameof(data.AverageDuration));

        data.CompletedItems.Should()
            .Be(sub.Sum(x => x.CompletedItems), nameof(data.CompletedItems));

        data.TotalItems.Should()
            .Be(sub.Sum(x => x.TotalItems), nameof(data.TotalItems));

        data.RemainingItems.Should().Be(sub.Sum(x => x.RemainingItems), nameof(data.RemainingItems));

        data.MaxDate.Should().Be(sub.Max(x => x.MaxDate), nameof(data.MaxDate));
        data.MinDate.Should().Be(sub.Min(x => x.MinDate), nameof(data.MinDate));
        sub.OfType<IDurationReportFactory>()
            .Select(x => x.Date)
            .Distinct()
            .Should()
            .HaveCountLessOrEqualTo(1);

        if (data.PercentComplete is 100 || data.PercentRemaining is 0)
        {
            scope.AppendTracing("PercentComplete is 100 or PercentRemaining is 0");
            data.RemainingTime.Should().Be(default, nameof(data.RemainingTime));
        }
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