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


    [SubAssertion]
    public void AssertMinBoundaries()
    {
        using var scope = new AssertionScope();
        // there should be no negative numbers since they don't make sense in the context of time here
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
        // related to subsets and fractions
        // possible to have 0/0 for many of these i.e. new accounts
        _report.CompletedItems.Should().BeLessThanOrEqualTo(_report.TotalItems);
        _report.AverageDuration.Should().BeLessThanOrEqualTo(_report.TotalTime);
        _report.CompletedTime.Should().BeLessThanOrEqualTo(_report.TotalTime);

        // we can have events that start and end on the same day
        _report.MaxDate.Should().BeOnOrAfter(_report.MinDate);


        // none done, no tasks: 100/0 or 0/0???
        // none done, >=1 task, : 0/100
        // these values are complements of each other
        new[] { _report.PercentComplete, _report.PercentRemaining }.Sum().Should().BeOneOf(default, 100);
    }

    public void AssertSubReportRelationalBoundaries()
    {
        var data = _report.Should().BeOfType<AggregateDurationReport>().Subject;
        using var scope = new AssertionScope();
        var sub = data.SubReports.Values().ToList();


        // we are taking the average of many averages in the aggregate
        // always true: min <= average >= max
        data
            .AverageDuration.Should()
            .BeLessThanOrEqualTo(sub.Max(x => x.AverageDuration), "avg <= max")
            .And.BeGreaterThanOrEqualTo(sub.Min(x => x.AverageDuration), "avg >= min");

        // we have fragments of one whole so they should all add up to the whole
        data
            .CompletedItems.Should()
            .Be(sub.Sum(x => x.CompletedItems),
                "sum of subreport values for completed items need to equal the aggregate's completed items"
            );
        data
            .TotalItems.Should()
            .Be(sub.Sum(x => x.TotalItems),
                "sum of subreport values for total items need to equal the aggregate's total items"
            );
        data
            .RemainingItems.Should()
            .Be(sub.Sum(x => x.RemainingItems),
                "sum of subreport values for remaining items need to equal the aggregate's remaining items"
            );


        // these are just the highest/lowest date values across all the items
        data
            .MaxDate.Should()
            .Be(sub.Max(x => x.MaxDate), "max of subreports' max dates should equal the aggregate's max date");
        data
            .MinDate.Should()
            .Be(sub.Min(x => x.MinDate), "min of subreports' min dates should equal the aggregate's min date");

        // not related to report calculation output
        // date refers to what "today" is to determine whether an event has completed
        // the data structure is designed to be self sufficient for querying only one type of entity
        // when we involve an aggregate context, we need to make sure the input date is the same for all reports or else "today" is different for some of them
        // the aggregate can contain only dtos or other aggregate dto / factories in a recursive data structure (therefore 0 IDurationReportFactories) which don't have dates
        // other aggregates don't implement IDurationReportFactory
        // this assertion doesn't apply when there's only dtos
        sub
            .OfType<IDurationReportFactory>()
            .Select(x => x.Date)
            .Distinct()
            .Should()
            .HaveCountLessOrEqualTo(1);

        // normally a bad idea to add conditionals to tests
        // add additional context information to reduce confusion with debugging the test
        if (data.PercentComplete is 100)
        {
            scope.AppendTracing("PercentComplete is 100");
            // when 100% complete, the user is done. there is no end to wait for
            data.RemainingTime.Should().Be(default, "when 100% complete, there is no remaining time to wait");
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

    [AttributeUsage(AttributeTargets.Method)]
    public class SubAssertionAttribute : Attribute;
}