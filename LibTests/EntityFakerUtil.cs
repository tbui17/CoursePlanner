using Bogus;
using Lib.Interfaces;
using Lib.Services.ReportService;

namespace LibTests;

public class EntityFakerUtil
{
    public DateTime Reference = DateTime.Now.Date;

    public Faker<T> CreateFaker<T>() where T : class, IDateTimeRangeEntity
    {
        var faker = new Faker<T>()
            .RuleFor(x => x.Start, f => f.Date.Past())
            .RuleFor(x => x.End, (f, x) => f.Date.Between(x.Start, x.Start.AddDays(1)));

        return faker;
    }

    public DurationReportFactory CreateReport<T>() where T : class, IDateTimeRangeEntity
    {
        var faker = CreateFaker<T>();
        var entities = faker.Generate(100).Cast<IDateTimeRangeEntity>().ToList();

        var fac = new DurationReportFactory
        {
            Entities = entities.ToList(),
            Date = Reference
        };

        return fac;
    }
}