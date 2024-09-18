using FluentAssertions;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests.ReportTests;

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


    [TestCaseSource(nameof(TestData))]
    public void PropertyTest(List<DurationReport> entities, DateTime reference)
    {

        var fac = new AggregateDurationReportFactory()
        {
            Date = reference,
            Reports = entities
        };

        var res = fac.Create();

        new ReportBoundaryUtil(res).AssertIDurationBoundaries();

    }

    private static IEnumerable<TestCaseData> TestData()
    {
        var reference = new DateTime(2010, 1, 1);


        var f = new EntityFakerFactory() { Reference = reference };


        return Enumerable.Range(0, 10)
            .Select(_ => new TestCaseData(new List<DurationReport>()
                        { f.CreateReport<Course>(), f.CreateReport<Assessment>(), f.CreateReport<Term>() },
                    reference
                )
            );
    }
}