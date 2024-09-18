using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;
using Lib.Utils;

namespace LibTests.ReportTests;

[TestFixture]
public class ReportTest : BaseDbTest
{
    [Test]
    public async Task AggregateDurationReport_HasValidProperties()
    {
        var service = Resolve<ReportService>();

        var res = await service.GetAggregateReport();
        new ReportBoundaryUtil(res).AssertIDurationBoundaries();
    }


    [Test]
    public async Task SubReports_HaveValidProperties()
    {
        var service = Resolve<ReportService>();


        var res = await service.GetAggregateReport();
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

        var rep = await service.GetAggregateReport();

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