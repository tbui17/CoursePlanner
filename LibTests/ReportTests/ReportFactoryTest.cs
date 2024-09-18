using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Services.ReportService;

namespace LibTests.ReportTests;

public class ReportFactoryTest
{
    [Test]
    public void AggregateFactoryCreate_Empty_ShouldNotThrow()
    {
        var fac = new AggregateDurationReportFactory();
        fac.Invoking(x => x.Create()).Should().NotThrow();
    }

    [Test]
    public void AggregateFactoryCreate_Empty_Is0CompletedTime()
    {
        var fac = new AggregateDurationReportFactory();
        var res = fac.Create();
        res.CompletedTime.Should().Be(default);
    }

    [Test]
    public void DurationReportCreate_Empty_ShouldNotThrow()
    {
        var fac = new DurationReportFactory();
        fac.Invoking(x => x.ToData()).Should().NotThrow();
    }

    [Test]
    public void Reports_Empty_ShouldBeWithinBounds()
    {

        var report = new DurationReportFactory();

        var aggReportEmpty = new AggregateDurationReportFactory().Create();


        var aggFac = new AggregateDurationReportFactory
        {
            Reports = [report]
        };

        var aggReport = aggFac.Create();

        using var _ = new AssertionScope();


        using (var scope = new AssertionScope())
        {
            scope.AddReportable("variable", (nameof(report)));
            new ReportBoundaryUtil(report.ToData()).AssertIDurationBoundaries();
        }

        using (var scope = new AssertionScope())
        {
            scope.AddReportable("variable", (nameof(aggReport)));
            new ReportBoundaryUtil(aggReport).AssertIDurationBoundaries();
        }

        using (var scope = new AssertionScope())
        {
            scope.AddReportable("variable", (nameof(aggReportEmpty)));
            new ReportBoundaryUtil(aggReportEmpty).AssertIDurationBoundaries();
        }
    }
}