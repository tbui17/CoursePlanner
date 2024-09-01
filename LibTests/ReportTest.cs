using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests;

[TestFixture]
public class ReportTest : BaseDbTest
{
    [Test]
    public async Task GetDurationReport_IDurationReport_HasValidProperties()
    {
        var service = Resolve<ReportService>();

        var res = await service.GetDurationReport();
        new ReportBoundaryUtil(res).AssertIDurationBoundaries();
    }


    [Test]
    public async Task GetDurationReport_SubReports_HasValidProperties()
    {
        var service = Resolve<ReportService>();


        var res = await service.GetDurationReport();
        res.Should()
            .BeAssignableTo<AggregateDurationReport>()
            .Which.SubReports.Should()
            .HaveCountGreaterThan(1)
            .And.Subject.Values.Should()
            .AllBeAssignableTo<IDurationReport>()
            .Which.Should()
            .AllSatisfy(x => new ReportBoundaryUtil(x).AssertIDurationBoundaries())
            .And.Subject.OfType<DurationReport>().Should().AllSatisfy(x =>
            {
                if (x.Type == typeof(Assessment))
                {
                    x.TotalTime.Should().BeGreaterThan(TimeSpan.Zero);
                }
            });
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

[SuppressMessage("ReSharper", "UnusedMember.Local")]
file class ReportBoundaryUtil(IDurationReport report)
{
    private readonly IDurationReport _report = report.Should().BeAssignableTo<IDurationReport>().Subject;

    [AttributeUsage(AttributeTargets.Method)]
    private class SubAssertionAttribute : Attribute;

    [SubAssertion]
    private void AssertMinBoundaries()
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
    private void AssertRelationalBoundaries()
    {
        using var _ = new AssertionScope();
        _report.CompletedItems.Should().BeLessThanOrEqualTo(_report.TotalItems);
        _report.AverageDuration.Should().BeLessThanOrEqualTo(_report.TotalTime);
        _report.CompletedTime.Should().BeLessThanOrEqualTo(_report.TotalTime);
        _report.MaxDate.Should().BeOnOrAfter(_report.MinDate);
        new[] { _report.PercentComplete, _report.PercentRemaining }.Sum().Should().BeOneOf(default, 100);
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