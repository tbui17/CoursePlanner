using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;
using Lib.Utils;

namespace LibTests;

[TestFixture]
public class ReportTest : BaseDbTest
{
    [Test]
    public async Task AggregateDurationReport_HasValidProperties()
    {
        var service = Resolve<ReportService>();

        var res = await service.GetDurationReport();
        new ReportBoundaryUtil(res).AssertIDurationBoundaries();
    }


    [Test]
    public async Task SubReports_HaveValidProperties()
    {
        var service = Resolve<ReportService>();


        var res = await service.GetDurationReport();
        using var _ = new AssertionScope();

        res.Should()
            .BeAssignableTo<AggregateDurationReport>()
            .Which.SubReports.Should()
            .HaveCountGreaterThan(1)
            .And.Subject.SelectMany(x => x)
            .Should()
            .AllBeAssignableTo<IDurationReport>()
            .Which.Should()
            .AllSatisfy(x => new ReportBoundaryUtil(x).AssertIDurationBoundaries());
    }

    [Test]
    public async Task AggregateReportProperties_InRelationToSubReports_ShouldBeWithinBounds()
    {
        var service = Resolve<ReportService>();

        var rep = await service.GetDurationReport();

        var sub = rep.Should()
            .BeAssignableTo<AggregateDurationReport>()
            .Which.SubReports.SelectMany(x => x)
            .ToList();

        using var _ = new AssertionScope();
        rep.MaxDate.Should().Be(sub.MaxOrDefault(x => x.MaxDate));
        rep.MinDate.Should().Be(sub.MinOrDefault(x => x.MinDate));
        rep.RemainingItems.Should()
            .Be(sub.SumOrDefault(x => x.RemainingItems));
        rep.TotalItems.Should()
            .Be(sub.SumOrDefault(x => x.TotalItems));
        rep.RemainingTime.Should()
            .BeLessThanOrEqualTo(sub.MaxOrDefault(x => x.RemainingTime))
            .And.BeGreaterThanOrEqualTo(sub.MinOrDefault(x => x.RemainingTime));
    }
}

public class ReportFactoryTest
{
    [Test]
    public void AggregateFactoryCreate_Empty_ShouldNotThrow()
    {
        var fac = new AggregateDurationReportFactory();
        fac.Invoking(x => x.Create()).Should().NotThrow();
    }

    [Test]
    public void DurationReportCreate_Empty_ShouldNotThrow()
    {
        var fac = new DurationReportFactory();
        fac.Invoking(x => x.Create()).Should().NotThrow();
    }

    [Test]
    public void Reports_Empty_ShouldBeWithinBounds()
    {
        var aggFac = new AggregateDurationReportFactory();
        var fac = new DurationReportFactory();

        var aggReportEmpty = aggFac.Create();

        var report = fac.Create();
        aggFac.Reports.Add(report);

        var aggReport = aggFac.Create();

        using var _ = new AssertionScope();

        new ReportBoundaryUtil(report).AssertIDurationBoundaries();
        new ReportBoundaryUtil(aggReport).AssertIDurationBoundaries();
        new ReportBoundaryUtil(aggReportEmpty).AssertIDurationBoundaries();
    }
}

public class AggregateReportTest
{
    [Test]
    public void RemainingTime_Future_IsDifferenceBetweenPresentAndMax()
    {
        var fiveYears = TimeSpan.FromDays(365 * 5);
        var twoYears = TimeSpan.FromDays(365 * 2);
        var now = DateTime.Now.Date;
        var fac = new AggregateDurationReportFactory
        {
            Reports =
            [
                new() { MaxDate = now.Add(fiveYears), Type = typeof(int) },
                new() { MaxDate = now.Add(twoYears) },
                new() { MaxDate = now.Add(-fiveYears), Type = typeof(string) },
            ],
            Date = now
        };

        var res = fac.Create();


        res.RemainingTime.Should().Be(fiveYears);
    }


    [Test]
    public void CompletedTime_IsDifferenceBetweenPresentAndMin()
    {
        var fiveYears = TimeSpan.FromDays(365 * 5);
        var twoYears = TimeSpan.FromDays(365 * 2);
        var now = DateTime.Now.Date;

        var fac = new AggregateDurationReportFactory
        {
            Reports =
            [
                new() { MinDate = now.Add(fiveYears), Type = typeof(int) },
                new() { MinDate = now.Add(twoYears) },
                new() { MinDate = now.Add(-twoYears), Type = typeof(string) },
            ],
            Date = now
        };

        var res = fac.Create();


        res.CompletedTime.Should().Be(twoYears);
    }
}

public class RegularReportTest
{
    [Test]
    public void RemainingTime_Future_IsDifferenceBetweenPresentAndMax()
    {
        var fiveYears = TimeSpan.FromDays(365 * 5);
        var twoYears = TimeSpan.FromDays(365 * 2);
        var now = DateTime.Now.Date;
        var fac = new DurationReportFactory()
        {
            Entities =
            [
                new Term()
                {
                    Start = now.Add(-fiveYears),
                    End = now.Add(-twoYears)
                },
                new Term()
                {
                    Start = now.Add(-twoYears),
                    End = now.Add(twoYears)
                },
                new Term()
                {
                    Start = now.Add(twoYears),
                    End = now.Add(fiveYears)
                }
            ],
            Date = now
        };

        var res = fac.Create();


        res.RemainingTime.Should().Be(fiveYears);
    }


    [Test]
    public void CompletedTime_IsDifferenceBetweenPresentAndMin()
    {
        TimeSpan Year(int i) => TimeSpan.FromDays(365 * i);
        var now = DateTime.Now.Date;

        var fac = new DurationReportFactory()
        {
            Entities =
            [
                new Term()
                {
                    Start = now.Add(Year(-3)),
                    End = now.Add(Year(-2))
                },
                new Term()
                {
                    Start = now.Add(Year(-2)),
                    End = now.Add(Year(2))
                },
                new Term()
                {
                    Start = now.Add(Year(2)),
                    End = now.Add(Year(5))
                }
            ],
            Date = now
        };


        var res = fac.Create();


        res.CompletedTime.Should().Be(Year(3));
    }


    [Test]
    public void MethodsRetrieved()
    {
        new ReportBoundaryUtil(new DurationReport()).GetSubAssertions().Should().NotBeEmpty();

    }
}

[SuppressMessage("ReSharper", "UnusedMember.Local")]
file class ReportBoundaryUtil(IDurationReport report)
{
    private readonly IDurationReport _report = report.Should().BeAssignableTo<IDurationReport>().Subject;

    [AttributeUsage(AttributeTargets.Method)]
    public class SubAssertionAttribute : Attribute;

    [SubAssertion]
    public void AssertMinBoundaries()
    {
        using var _ = new AssertionScope();
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
        _report.CompletedTime.Should().BeLessThan(_report.TotalTime);
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