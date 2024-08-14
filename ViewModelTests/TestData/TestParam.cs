using System.Collections;
using Lib.Interfaces;

namespace ViewModelTests.TestData;

public class TestParam
{
    public static IEnumerable<TestCaseData> NameAndDate()
    {
        return new[]
        {
            new TestCaseData(
                "",
                DateTime.Now,
                DateTime.Now.AddDays(1)
            ).SetName("EmptyName"),
            new TestCaseData(
                "Valid Name",
                DateTime.Now.AddDays(1),
                DateTime.Now
            ).SetName("StartAfterEnd")
        };
    }
}

public record NameAndDate : IEntity, IDateTimeRange
{
    public int Id { get; set; }
    public string Name { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
}