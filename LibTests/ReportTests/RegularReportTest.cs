using Bogus;
using FluentAssertions;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests.ReportTests;

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

        var res = fac.ToData();


        res.RemainingTime.Should().Be(fiveYears);
    }


    [TestCaseSource(nameof(TestData))]
    public void Properties_Pbt_ShouldBeWithinConstraints(List<IDateTimeRangeEntity> entities, DateTime reference)
    {
        var fac = new DurationReportFactory
        {
            Entities = entities.ToList(),
            Date = reference
        };

        var report = fac.ToData();
        new ReportBoundaryUtil(report).AssertBoundaries();
    }

    private static IEnumerable<TestCaseData> TestData()
    {
        // "today" - 2010
        var reference = new DateTime(2010, 1, 1);

        var faker = new Faker<Course>()
            .RuleFor(x => x.Start, f => f.Date.Between(new DateTime(2000, 1, 1), new DateTime(2020, 1, 1)))
            // start date always <= end date
            .RuleFor(x => x.End, (f, x) => f.Date.Between(x.Start, new DateTime(2020, 12, 1)))
            // irrelevant but failed tests involving these will indicate unexpected interaction between the factory and these fields
            .RuleFor(x => x.ShouldNotify, f => f.Random.Bool())
            .RuleFor(x => x.Name, f => f.Lorem.Sentences(3));


        return Enumerable
            .Range(0, 100)
            .Select(_ =>
                {
                    var courses = faker.Generate(100).Cast<IDateTimeRangeEntity>().ToList();


                    return new TestCaseData(courses, reference);
                }
            );
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


        var res = fac.ToData();


        res.CompletedTime.Should().Be(Year(3));
    }
}