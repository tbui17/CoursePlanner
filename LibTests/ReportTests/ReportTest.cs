using FluentAssertions;
using FluentAssertions.Execution;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests.ReportTests;

[TestFixture]
public class ReportTest : BaseDbTest
{
    [Test]
    public async Task AggregateDurationReport_HasValidProperties()
    {
        var service = Resolve<ReportService>();

        var res = await service.GetAggregateDurationReportData();
        using var scope = new AssertionScope();
        scope.AddReportable("data", res.Serialize);
        new ReportBoundaryUtil(res).AssertBoundaries();
    }

    [Test]
    public async Task SubReports_HaveValidProperties()
    {
        var service = Resolve<ReportService>();


        var res = await service.GetAggregateDurationReportData();

        using var scope = new AssertionScope();
        scope.AddReportable("data", res.Serialize);

        res
            .Should()
            .BeAssignableTo<AggregateDurationReport>()
            .Which.SubReports.Should()
            .HaveCountGreaterThan(1)
            .And.Subject.SelectMany(x => x)
            .Should()
            .AllBeAssignableTo<IDurationReport>()
            .Which.Should()
            .AllSatisfy(x => new ReportBoundaryUtil(x).AssertBoundaries());
    }

    [Test]
    public async Task AggregateReportProperties_InRelationToSubReports_ShouldBeWithinBounds()
    {
        var service = Resolve<ReportService>();

        var rep = await service.GetAggregateDurationReportData(DateTime.MinValue);


        using var scope = new AssertionScope();
        scope.AddReportable("data", rep.Serialize);

        var util = new ReportBoundaryUtil(rep);
        util.AssertBoundaries();
        util.AssertSubReportRelationalBoundaries();
    }
}