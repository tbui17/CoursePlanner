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
                DateTime.Now.AddMinutes(1)
            ).SetName("EmptyName"),
            new TestCaseData(
                "   ",
                DateTime.Now,
                DateTime.Now.AddMinutes(1)
            ).SetName("Whitespace"),
            new TestCaseData(
                "Valid Name",
                DateTime.Now.AddMinutes(1),
                DateTime.Now
            ).SetName("StartAfterEnd"),
            new TestCaseData(
                "",
                DateTime.Now.AddMinutes(1),
                DateTime.Now
            ).SetName("AllInvalid"),
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