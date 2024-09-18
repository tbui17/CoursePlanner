using Bogus;
using Lib.Interfaces;
using Lib.Models;
using Lib.Services.ReportService;

namespace LibTests;

public class EntityFakerFactory
{
    public DateTime Reference = DateTime.Now.Date;

    public Faker<T> CreateEntityFaker<T>() where T : class, IDateTimeEntity
    {
        var faker = new Faker<T>()
            .RuleFor(x => x.Start, f => f.Date.Past())
            .RuleFor(x => x.End, (f, x) => f.Date.Between(x.Start, x.Start.AddDays(1)));

        return faker;
    }

    public DurationReport CreateReport<T>() where T : class, IDateTimeEntity
    {
        var faker = CreateEntityFaker<T>();
        var entities = faker.Generate(100).Cast<IDateTimeEntity>().ToList();

        var fac = new DurationReportFactory
        {
            Entities = entities.ToList(),
            Date = Reference
        };

        var report = fac.Create();
        return report;
    }
}