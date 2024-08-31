using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests;

[TestFixture]
public class ReportTest
{
    [Test]
    public async Task GetDurationReport_IDurationReport_HasValidProperties()
    {
        var service = Resolve<ReportService>();

        var res = await service.GetDurationReport();
        AssertIDurationBoundaries(res);
    }

    private static void AssertIDurationBoundaries(IDurationReport report)
    {
        var s = report.Should().BeAssignableTo<IDurationReport>().Subject;
        using var _ = new AssertionScope();
        AssertIDurationMinBoundaries(report);
        s.CompletedItems.Should().BeGreaterThanOrEqualTo(default).And.BeLessThanOrEqualTo(s.TotalItems);
        s.AverageDuration.Should().BeLessThanOrEqualTo(s.TotalTime);
        s.CompletedTime.Should().BeLessThanOrEqualTo(s.TotalTime);
        s.MaxDate.Should().BeOnOrAfter(s.MinDate);
        s.RemainingTime.Should().BeGreaterThanOrEqualTo(default);
        new[] { s.PercentComplete, s.PercentRemaining }.Sum().Should().BeOneOf(default, 100);
    }

    private static void AssertIDurationMinBoundaries(IDurationReport report)
    {
        var s = report.Should().BeAssignableTo<IDurationReport>().Subject;
        using var _ = new AssertionScope();
        s.CompletedTime.Should().BeGreaterThanOrEqualTo(default);
        s.AverageDuration.Should().BeGreaterThanOrEqualTo(default);
        s.RemainingTime.Should().BeGreaterThanOrEqualTo(default);
        s.TotalTime.Should().BeGreaterThanOrEqualTo(default);
        s.CompletedItems.Should().BeGreaterThanOrEqualTo(default);
        s.PercentComplete.Should().BeGreaterThanOrEqualTo(default);
        s.PercentRemaining.Should().BeGreaterThanOrEqualTo(default);
        s.RemainingItems.Should().BeGreaterThanOrEqualTo(default);
        s.TotalItems.Should().BeGreaterThanOrEqualTo(default);
        s.MaxDate.Should().BeOnOrAfter(default);
        s.MinDate.Should().BeOnOrAfter(default);

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
            .AllSatisfy(AssertIDurationBoundaries);
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
}