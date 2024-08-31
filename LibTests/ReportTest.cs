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
        AssertIDuration(res);
    }

    private void AssertIDuration(IDurationReport report)
    {
        var s = report.Should().BeAssignableTo<IDurationReport>().Subject;
        using var scope = new AssertionScope();

        s.CompletedItems.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(s.TotalItems);
        s.AverageDuration.Should().BeLessThanOrEqualTo(s.TotalTime);
        s.MaxDate.Should().BeOnOrAfter(s.MinDate);
        s.RemainingTime.Should().BeGreaterThanOrEqualTo(TimeSpan.Zero);
        new[] { s.PercentComplete, s.PercentRemaining }.Sum().Should().Be(100);
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
            .AllSatisfy(AssertIDuration);
    }
}