using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests.ReportTests;



public class AggregateReportTest
{

    private record DurationReportFactoryFake : IDurationReportFactory
    {
        public TimeSpan TotalTime { get; set; }
        public TimeSpan CompletedTime { get; set; }
        public TimeSpan RemainingTime { get; set; }
        public TimeSpan AverageDuration { get; set; }
        public DateTime MinDate { get; set; }
        public DateTime MaxDate { get; set; }
        public int TotalItems { get; set; }
        public int CompletedItems { get; set; }
        public int RemainingItems { get; set; }
        public double PercentComplete { get; set; }
        public double PercentRemaining { get; set; }
        public IReadOnlyCollection<IDateTimeRange> Entities { get; init; }
        public DateTime Date { get; init; }
        public Type Type { get; set; }

        public DurationReport ToData()
        {
            return new DurationReport();
        }
    }

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
                new DurationReportFactoryFake() { MaxDate = now.Add(fiveYears), Type = typeof(int), Date = now },
                new DurationReportFactoryFake() { MaxDate = now.Add(twoYears), Date = now },
                new DurationReportFactoryFake() { MaxDate = now.Add(-fiveYears), Type = typeof(string), Date = now },
            ]
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
                new DurationReportFactoryFake { MinDate = now.Add(fiveYears), Type = typeof(int), MaxDate = DateTime.MaxValue, Date = now},
                new DurationReportFactoryFake { MinDate = now.Add(twoYears), MaxDate = DateTime.MaxValue, Date=now},
                new DurationReportFactoryFake { MinDate = now.Add(-twoYears), Type = typeof(string), MaxDate = DateTime.MaxValue,Date=now },
            ]
        };

        var res = fac.Create();


        res.CompletedTime.Should().Be(twoYears);
    }


    [TestCaseSource(nameof(TestData))]
    public void PropertyTest(List<DurationReportFactory> entities, DateTime reference)
    {
        var fac = new AggregateDurationReportFactory
        {
            Reports = entities
        };

        var res = fac.Create();

        res.Dump();

        new ReportBoundaryUtil(res).AssertIDurationBoundaries();
    }

    private static IEnumerable<TestCaseData> TestData()
    {
        var reference = new DateTime(2010, 1, 1);


        var f = new EntityFakerUtil { Reference = reference };


        return Enumerable.Range(0, 10)
            .Select(i =>
                {
                    return new TestCaseData(
                        new[]
                            { f.CreateReport<Course>(), f.CreateReport<Assessment>(), f.CreateReport<Term>() }.ToList(),
                        reference
                    ).SetName(i.ToString());
                }
            );
    }
}