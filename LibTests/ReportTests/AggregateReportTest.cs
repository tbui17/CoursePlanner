using FluentAssertions;
using FluentAssertions.Extensions;
using Lib.Interfaces;
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
                new DurationReportFactoryStub { MaxDate = now.Add(fiveYears), Type = typeof(int), Date = now },
                new DurationReportFactoryStub { MaxDate = now.Add(twoYears), Date = now },
                new DurationReportFactoryStub { MaxDate = now.Add(-fiveYears), Type = typeof(string), Date = now },
            ]
        };

        var res = fac.Create();


        res.RemainingTime.Should().Be(fiveYears);
    }


    [Test]
    public void CompletedTime_IsDifferenceBetweenPresentAndStartDateOfOldestEntry()
    {
        var fiveYears = TimeSpan.FromDays(365 * 5);
        var twoYears = TimeSpan.FromDays(365 * 2);
        var now = DateTime.Now.Date;
        var oneYear = TimeSpan.FromDays(365);

        // given a collection of reports where the oldest date is 2 years ago and the second oldest is 1 year ago
        var fac = new AggregateDurationReportFactory
        {
            Reports =
            [
                new DurationReportFactoryStub
                    { MinDate = now.Add(fiveYears), Type = typeof(int), MaxDate = DateTime.MaxValue, Date = now },
                new DurationReportFactoryStub { MinDate = now.Add(twoYears), MaxDate = DateTime.MaxValue, Date = now },
                new DurationReportFactoryStub // oldest min date
                {
                    MinDate = now.Add(-twoYears), Type = typeof(string), MaxDate = DateTime.MaxValue, Date = now
                },
                new DurationReportFactoryStub // second oldest min date
                    { MinDate = now.Add(-oneYear), Type = typeof(int), MaxDate = DateTime.MaxValue, Date = now }
            ]
        };

        // when completed time is calculated
        var res = fac.Create();


        // then completed time should be 2 years, because 2 years of time have elapsed since the start date of the oldest item
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

        new ReportBoundaryUtil(res).AssertBoundaries();
    }

    private static IEnumerable<TestCaseData> TestData()
    {
        var reference = new DateTime(2010, 1, 1);


        var f = new EntityFakerUtil { Reference = reference };


        return Enumerable
            .Range(0, 10)
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

public class AggregateReportIgnoreEmptyTypeTest
{
    private AggregateReportStub _expectedAgg;
    private DateTime _reference;
    private DurationReportFactory _termReports;

    [SetUp]
    public void Setup()
    {
        var reference = new DateTime(2010, 1, 1);
        var expectedAgg = new AggregateReportStub
        {
            // if 50 days have passed, then 50 days remain
            RemainingTime = 50.Days(),
            CompletedTime = 50.Days(),

            // only 1 item
            TotalItems = 1,

            // the 1 item started 50 days ago
            // the 1 item ends in 50 days
            MinDate = reference.AddDays(-50),
            MaxDate = reference.AddDays(50),

            // the 1 item is not complete
            RemainingItems = 1,
            CompletedItems = 0,

            // no items done
            PercentComplete = 0,
            PercentRemaining = 100,

            // The date range spans over 100 days
            TotalTime = 100.Days(),

            // if there's only one type, then the average across all types is just the one type's average
            // the average of a single item collection is simply the one item's value
            AverageDuration = 100.Days(),
        };

        // the item started 50 days ago and ends in 50 days
        // 50 day progress out of 100 days
        var term = new Term
        {
            Start = reference.AddDays(-50),
            End = reference.AddDays(50)
        };
        var termReports = new DurationReportFactory
        {
            Entities = [term],
            Date = reference,
            Type = typeof(Term)
        };

        _expectedAgg = expectedAgg;
        _termReports = termReports;
        _reference = reference;
    }

    [Test]
    public void Properties_OneTypeOfOneTotalType50OutOf100DayProgress_HasExpectedValues()
    {
        var aggReports = new AggregateDurationReportFactory
        {
            Reports = [_termReports]
        };

        // additional props of left variable are ignored
        aggReports.Should().BeEquivalentTo(_expectedAgg);
    }

    // prevents aggregate calculations from accepting defaulted values like 01/01/0001 on empty sub reports and give weird values like 50000 complete days/50001 total days
    // occurs when only some types have data
    [Test]
    public void Properties_TwoOfThreeTypesEmpty_CalculationIgnoresDataFromEmptyTypes()
    {
        var courseReports = new DurationReportFactory
        {
            Date = _reference,
            Type = typeof(Course),
            Entities = []
        };
        var assessmentReports = new DurationReportFactory
        {
            Date = _reference,
            Type = typeof(Assessment),
            Entities = []
        };

        var aggReports = new AggregateDurationReportFactory
        {
            Reports = [_termReports, courseReports, assessmentReports]
        };

        // if types with empty data are ignored, then this should be the same case as the previous test
        aggReports.Should().BeEquivalentTo(_expectedAgg);
    }
}

public record DurationReportFactoryStub : IDurationReportFactory
{
    public TimeSpan TotalTime { get; set; }
    public TimeSpan CompletedTime { get; set; }
    public TimeSpan RemainingTime { get; set; }
    public TimeSpan AverageDuration { get; set; }
    public DateTime MinDate { get; set; }

    public DateTime MaxDate { get; set; }

    // aggregate ignores if total count is 0
    public int TotalItems { get; set; } = 1;
    public int CompletedItems { get; set; }
    public int RemainingItems { get; set; }
    public double PercentComplete { get; set; }
    public double PercentRemaining { get; set; }
    public IReadOnlyCollection<IDateTimeRange> Entities { get; init; } = null!;
    public DateTime Date { get; init; }
    public Type Type { get; set; } = null!;

    public DurationReport ToData()
    {
        return new DurationReport();
    }
}

public record AggregateReportStub : IDurationReport
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
}